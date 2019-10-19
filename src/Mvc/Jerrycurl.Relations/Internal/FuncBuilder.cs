using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Internal.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jerrycurl.Linq.Expressions;

namespace Jerrycurl.Relations.Internal
{
    internal class FuncBuilder
    {
        private delegate void ReadFields(IField model, IField source, MetadataIdentity[] attributes, Delegate[] binders, IEnumerator[] enumerators, IField[] fields, IMetadataNotation notation);

        private readonly FuncArguments args;
        private readonly ExpressionOptimizer optimizer = new ExpressionOptimizer();
        private readonly RelationNode relationNode;

        public FuncBuilder(RelationNode relationNode)
        {
            this.relationNode = relationNode;
            this.args = new FuncArguments(relationNode.Items.Count);
        }

        public FuncDescriptor Build()
        {
            Delegate[] binders = this.relationNode.Fields.Select(this.CreateBinder).ToArray();
            MetadataIdentity[] attributes = this.relationNode.Attributes;
            IMetadataNotation notation = attributes[0].Schema.Notation;

            Action<IField, IEnumerator[], IField[]>[] funcs = new Action<IField, IEnumerator[], IField[]>[this.relationNode.Items.Count];
            MetadataIdentity[] listIds = new MetadataIdentity[this.relationNode.Items.Count];

            for (int i = 0; i < funcs.Length; i++)
            {
                ReadFields itemReader = this.GetItemReader(this.relationNode.Items[i]);

                funcs[i] = (source, enumerators, fields) => itemReader(source.Model, source, attributes, binders, enumerators, fields, notation);
                listIds[i] = this.relationNode.Items[i].List?.Metadata.Identity ?? this.relationNode.Items[i].Metadata.Identity;
            }

            return new FuncDescriptor()
            {
                Factories = funcs,
                Degree = this.relationNode.Degree,
                VisibleDegree = this.relationNode.VisibleDegree,
                Identity = this.relationNode.Identity,
                Lists = listIds,
            };
        }

        private Expression GetAssignEnumeratorExpression(Expression list, ListNode listNode)
        {
            Type enumeratorType = this.GetIndexedEnumeratorType(listNode);
            ConstructorInfo constructor = enumeratorType.GetConstructors()[0];

            Expression newEnumerator = Expression.New(constructor, list);
            Expression arrayAccess = Expression.ArrayAccess(this.args.Enumerators, Expression.Constant(listNode.EnumeratorIndex));

            return Expression.Assign(arrayAccess, newEnumerator);
        }

        private Type GetIndexedEnumeratorType(ListNode listNode)
        {
            if (listNode.HasFlag(NodeFlags.Product))
                return typeof(ProductEnumerator<>).MakeGenericType(listNode.Item.Metadata.Type);
            else
                return typeof(IndexedEnumerator<>).MakeGenericType(listNode.Item.Metadata.Type);
        }

        private ReadFields GetItemReader(ItemNode itemNode)
        {
            Expression parent = this.GetParentValueExpression(itemNode);
            Expression itemName = this.GetAssignItemNameExpression(itemNode);
            Expression fields = this.GetAssignFieldExpression(parent, itemNode, itemNode);
            Expression body = Expression.Block(itemName, fields);
            Expression optimized = this.optimizer.BuildBlock(body, new[] { this.args.ItemVars[itemNode.ItemIndex] });

            Expression<ReadFields> lambda = Expression.Lambda<ReadFields>(optimized, this.args.GetParameters());

            return lambda.Compile();
        }

        private Expression GetParentValueExpression(ItemNode itemNode)
        {
            if (itemNode.List != null)
                return this.GetValueExpression(null, itemNode.List);

            return null;
        }

        private Expression GetAttributeExpression(MemberNode memberNode)
        {
            if (memberNode.HasFlag(NodeFlags.Source))
                return Expression.Property(this.args.Source, "Attribute");

            return Expression.ArrayAccess(this.args.Attributes, Expression.Constant(memberNode.FieldIndex.Value));
        }

        private Expression GetAssignFieldExpression(Expression parent, MemberNode memberNode, ItemNode itemNode)
        {
            Expression value = this.GetValueExpression(parent, memberNode);

            List<Expression> expressions = new List<Expression>();

            if (memberNode.HasFlag(NodeFlags.Field | NodeFlags.Source))
            {
                Expression arrayAccess = Expression.ArrayAccess(this.args.Fields, Expression.Constant(memberNode.FieldIndex.Value));
                Expression assign = Expression.Assign(arrayAccess, this.args.Source);

                expressions.Add(assign);
            }
            else if (memberNode.HasFlag(NodeFlags.Field))
            {
                Expression arrayAccess = Expression.ArrayAccess(this.args.Fields, Expression.Constant(memberNode.FieldIndex.Value));

                Expression name = this.GetNameExpression(itemNode, memberNode);
                Expression attribute = this.GetAttributeExpression(memberNode);
                Expression index = this.GetEnumeratorIndex(itemNode.List);
                Expression binder = this.GetBinderExpression(memberNode);
                Expression model = this.args.Model;

                ConstructorInfo constructor = typeof(Field<,>).MakeGenericType(value.Type, parent.Type).GetConstructors()[0];

                Expression newField = Expression.New(constructor, name, attribute, value, parent, binder, index, model);
                Expression assign = Expression.Assign(arrayAccess, newField);

                expressions.Add(assign);
            }

            if (memberNode.Members.Count > 0)
            {
                Expression[] memberExpressions = memberNode.Members.Select(n => this.GetAssignFieldExpression(value, n, itemNode)).ToArray();
                Expression[] missingExpressions = this.GetFieldNodes(memberNode).Select(n => this.GetMissingExpression(itemNode, n)).ToArray();

                if (this.IsNotNullableValueType(value.Type))
                    expressions.Add(this.GetBlockOrExpression(memberExpressions));
                else
                {
                    Expression isNull = Expression.ReferenceEqual(value, Expression.Constant(null, value.Type));
                    Expression ifTrue = this.GetBlockOrExpression(missingExpressions);
                    Expression ifFalse = this.GetBlockOrExpression(memberExpressions);
                    Expression ifBlock = Expression.IfThenElse(isNull, ifTrue, ifFalse);

                    expressions.Add(ifBlock);
                }
            }

            if (memberNode.HasFlag(NodeFlags.List))
            {
                ListNode listNode = (ListNode)memberNode;

                expressions.Add(this.GetAssignEnumeratorExpression(value, listNode));
            }

            return this.GetBlockOrExpression(expressions);
        }

        private Expression GetBlockOrExpression(IEnumerable<Expression> expressions, Type defaultType = null)
        {
            if (!expressions.Any())
                return Expression.Default(defaultType ?? typeof(void));
            else if (expressions.Count() == 1)
                return expressions.First();

            return Expression.Block(expressions);
        }

        private IEnumerable<MemberNode> GetFieldNodes(MemberNode node)
        {
            return node.EnumerateNodes().Skip(1).Where(n => n.HasFlag(NodeFlags.Field));
        }

        private Expression GetMissingExpression(ItemNode itemNode, MemberNode memberNode)
        {
            Expression arrayAccess = Expression.ArrayAccess(this.args.Fields, Expression.Constant(memberNode.FieldIndex.Value));

            Expression name = this.GetNameExpression(itemNode, memberNode);
            Expression attribute = this.GetAttributeExpression(memberNode);
            Expression model = this.args.Model;

            ConstructorInfo constructor = typeof(Missing<>).MakeGenericType(memberNode.Metadata.Type).GetConstructors()[0];

            Expression newField = Expression.New(constructor, name, attribute, model);

            return Expression.Assign(arrayAccess, newField);
        }

        private Type GetBinderType(MemberNode memberNode)
        {
            return typeof(Action<,,>).MakeGenericType(memberNode.Metadata.Parent.Type, typeof(int), memberNode.Metadata.Type);
        }

        private Expression GetBinderExpression(MemberNode memberNode)
        {
            Type binderType = this.GetBinderType(memberNode);

            Expression arrayAccess = Expression.ArrayAccess(this.args.Binders, Expression.Constant(memberNode.FieldIndex.Value));

            return Expression.Convert(arrayAccess, binderType);
        }

        private Expression GetEnumeratorIndex(ListNode listNode)
        {
            if (listNode == null)
                return Expression.Constant(0);

            Expression enumerator = this.GetEnumeratorExpression(listNode);

            return Expression.Property(enumerator, "Index"); ;
        }

        private Expression GetEnumeratorExpression(ListNode listNode)
        {
            Type enumeratorType = this.GetIndexedEnumeratorType(listNode);

            Expression arrayAccess = Expression.ArrayAccess(this.args.Enumerators, Expression.Constant(listNode.EnumeratorIndex));
            Expression enumerator = Expression.Convert(arrayAccess, enumeratorType);

            return enumerator;
        }


        private Expression GetFieldExpression(MemberNode memberNode)
        {
            if (memberNode.HasFlag(NodeFlags.Source))
                return this.args.Source;
            else if (memberNode.FieldIndex != null)
                return Expression.ArrayAccess(this.args.Fields, Expression.Constant(memberNode.FieldIndex.Value));

            throw new InvalidOperationException();
        }

        private Expression GetAssignItemNameExpression(ItemNode itemNode)
        {
            Expression itemName = this.GetItemNameExpression(itemNode);
            Expression variable = this.args.ItemVars[itemNode.ItemIndex] = Expression.Parameter(typeof(string), "itemName" + itemNode.ItemIndex);

            return Expression.Assign(variable, itemName);
        }

        private Expression GetItemNameExpression(ItemNode itemNode)
        {
            Expression nameExpression;

            if (itemNode.HasFlag(NodeFlags.Source))
                nameExpression = Expression.Property(Expression.Property(this.args.Source, "Identity"), "Name");
            else if (itemNode.List != null)
            {
                Expression enumerator = this.GetEnumeratorExpression(itemNode.List);
                Expression list = this.GetFieldExpression(itemNode.List);
                Expression listName = Expression.Property(Expression.Property(list, "Identity"), "Name");
                Expression itemName = Expression.Constant(itemNode.Metadata.Identity.Notation.Path(itemNode.List.Metadata.Identity.Name, itemNode.Metadata.Identity.Name));
                Expression combined = this.GetNotationCombine(listName, itemName);
                Expression index = Expression.Property(enumerator, "Index");
                Expression indexed = this.GetNotationIndex(combined, index);

                nameExpression = indexed;
            }
            else
                throw new InvalidOperationException();

            return nameExpression;
        }


        private Expression GetNameExpression(ItemNode itemNode, MemberNode memberNode)
        {
            Expression itemName = this.args.ItemVars[itemNode.ItemIndex];
            Expression memberName;

            if (memberNode.HasFlag(NodeFlags.Item))
                memberName = itemName;
            else
            {
                Expression name = Expression.Constant(memberNode.Metadata.Identity.Notation.Path(itemNode.Metadata.Identity.Name, memberNode.Metadata.Identity.Name));

                memberName = this.GetNotationCombine(itemName, name);
            }

            return memberName;
        }

        private Expression GetNotationCombine(params Expression[] parts)
        {
            Type notationType = this.args.Notation.Type;
            MethodInfo combineMethod = notationType.GetMethod("Combine", new[] { typeof(string), typeof(string) });

            return Expression.Call(this.args.Notation, combineMethod, parts);
        }

        private Expression GetNotationIndex(Expression name, Expression index)
        {
            Type notationType = this.args.Notation.Type;
            MethodInfo indexMethod = notationType.GetMethod("Index");

            return Expression.Call(this.args.Notation, indexMethod, name, index);
        }

        private Expression GetValueExpression(Expression parent, MemberNode memberNode)
        {
            Expression value;

            if (memberNode is ItemNode itemNode && itemNode.List != null)
            {
                Expression enumerator = this.GetEnumeratorExpression(itemNode.List);

                value = Expression.Property(enumerator, "Current");
            }
            else if (memberNode.HasFlag(NodeFlags.Source))
            {
                Expression rawValue = Expression.Property(this.args.Source, "Value");

                value = Expression.Convert(rawValue, memberNode.Metadata.Type);
            }
            else if (parent != null)
                return Expression.MakeMemberAccess(parent, memberNode.Metadata.Member);
            else if (memberNode.FieldIndex != null)
            {
                Expression field = this.GetFieldExpression(memberNode);
                Expression rawValue = Expression.Property(field, "Value");

                value = Expression.Convert(rawValue, memberNode.Metadata.Type);
            }
            else
                throw new InvalidOperationException();

            this.optimizer.Add(value, memberNode.Metadata, "value");

            return value;
        }

        private bool IsNotNullableValueType(Type t)
        {
            return (t.IsValueType && Nullable.GetUnderlyingType(t) == null);
        }

        #region " Binders "
        private Delegate CreateBinder(MemberNode fieldNode)
        {
            if (fieldNode.HasFlag(NodeFlags.Source))
                return null;

            Type binderType = this.GetBinderType(fieldNode);

            ParameterExpression parent = Expression.Parameter(fieldNode.Metadata.Parent.Type);
            ParameterExpression index = Expression.Parameter(typeof(int));
            ParameterExpression value = Expression.Parameter(fieldNode.Metadata.Type);

            Expression bind;

            if (fieldNode.HasFlag(NodeFlags.Item))
            {
                if (fieldNode.Metadata.WriteIndex != null)
                    bind = Expression.Call(parent, fieldNode.Metadata.WriteIndex, index, value);
                else
                    bind = Expression.Throw(Expression.New(typeof(NotIndexableException)));
            }
            else if (fieldNode.Metadata.Member is PropertyInfo pi && !pi.CanWrite)
                bind = Expression.Throw(Expression.New(typeof(NotWritableException)));
            else if (fieldNode.Metadata.Member != null)
                bind = Expression.Assign(Expression.MakeMemberAccess(parent, fieldNode.Metadata.Member), value);
            else
                return null;

            return Expression.Lambda(binderType, bind, new[] { parent, index, value }).Compile();
        }
        #endregion  
    }
}
