using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Nodes;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryCompiler
    {
        public QueryIndexer Indexer { get; }

        public QueryCompiler(QueryIndexer indexer)
        {
            this.Indexer = indexer;
        }

        public QueryCache<TItem> Compile<TItem>(ValueNode valueNode)
        {
            Scope scope = new Scope();

            return null;
        }

        private Expression GetItemExpression(ItemNode node)
        {
            Expression value = this.GetValueExpression(node.Value);
            Expression list = this.GetArrayIndexExpression(Scope.Lists, node.List.ListIndex);

            return null;
        }

        private Expression GetValueExpression(ValueNode node)
        {
            return node switch
            {
                ScalarNode scalarNode => this.GetScalarExpression(scalarNode),
                JoinNode joinNode => this.GetJoinExpression(joinNode),
                _ => this.GetContainerExpression(node),
            };
        }

        private Expression GetContainerExpression(ValueNode node)
        {
            NewExpression newExpression = node.Metadata.Composition.Construct;

            return Expression.MemberInit(newExpression, node.Properties.Select(this.GetValueMemberBinding));
        }

        private Expression GetScalarExpression(ScalarNode node)
        {
            return null;
        }

        private Expression GetJoinExpression(JoinNode node)
        {
            return null;
        }

        private void AddHelperVariables(IEnumerable<HelperNode> nodes, List<Expression> body)
        {
            foreach (HelperNode node in nodes)
            {
                ParameterExpression variable = this.variables.Add($"helper{node.Index}", node.Type);

                Expression arrayIndex = this.GetArrayIndexExpression(Scope.Helpers, node.Index);
                Expression castValue = Expression.Convert(arrayIndex, variable.Type);
                Expression assignVar = Expression.Assign(variable, castValue);

                body.Add(assignVar);
            }
        }

        private Expression GetDynamicExpression(ValueNode node)
        {
            ParameterExpression variable = Expression.Variable(node.Metadata.Composition.Construct.Type);
            NewExpression newExpression = node.Metadata.Composition.Construct;

            List<Expression> body = new List<Expression>()
            {
                Expression.Assign(variable, newExpression),
            };

            foreach (ValueNode propertyNode in node.Properties)
            {
                string memberName = this.State.Schema.Notation.Member(propertyNode.Identity.Name);

                Expression value = this.GetValueExpression(propertyNode);

                if (value.Type.IsValueType)
                    value = Expression.Convert(value, typeof(object));

                Expression addCall = Expression.Call(variable, node.Metadata.Composition.AddDynamic, Expression.Constant(memberName), value);

                body.Add(addCall);
            }

            body.Add(variable);

            return Expression.Block(new[] { variable }, body);
        }

        private Expression GetScalarIsDbNullExpression(ScalarNode node)
        {
            MethodInfo isDbNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            return Expression.Call(Scope.DataReader, isDbNullMethod, Expression.Constant(node.Column.Index));
        }

        private Expression GetScalarValueExpression(ScalarNode node)
        {
            MethodInfo readMethod = this.GetScalarValueMethod(node);

            Expression index = Expression.Constant(node.Column.Index);
            Expression dataReader = Scope.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            return Expression.Call(dataReader, readMethod, index);
        }

        private MethodInfo GetScalarValueMethod(ScalarNode node)
        {
            BindingColumnInfo bindingInfo = new BindingColumnInfo()
            {
                Metadata = node.Metadata,
                Column = node.Column,
            };

            MethodInfo readMethod = node.Metadata.Value?.Read?.Invoke(bindingInfo);

            if (readMethod == null)
                readMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });

            return readMethod;
        }

        private Expression GetArrayIndexExpression(Expression arrayExpression, int index)
        {
            PropertyInfo indexer = arrayExpression.Type.GetProperty("Item");

            return Expression.Property(arrayExpression, indexer, Expression.Constant(index));
        }

        private Type GetDictionaryType(KeyNode node)
        {
            Type compositeType = this.GetCompositeKeyType(node);

            return typeof(Dictionary<,>).MakeGenericType(compositeType, typeof(ElasticArray));
        }

        private Type GetCompositeKeyType(KeyNode node)
        {
            return GetKeyType(node.Type);

            static Type GetKeyType(ICollection<Type> keyType)
            {
                if (keyType.Count == 0)
                    return null;
                else if (keyType.Count <= 4)
                    return typeof(CompositeKey<>).MakeGenericType(keyType.Take(4).ToArray());
                else
                {
                    Type[] fullType = keyType.Take(4).Concat(new[] { GetKeyType(keyType.Skip(4).ToArray()) }).ToArray();

                    return typeof(CompositeKey<,,,,>).MakeGenericType(fullType);
                }
            }
        }

        private MemberAssignment GetValueMemberBinding(ValueNode node)
        {
            if (!node.Metadata.HasFlag(BindingMetadataFlags.Writable))
                throw BindingException.FromMetadata(node.Metadata, "Cannot bind to read-only property.");

            Expression value = this.GetValueExpression(node);

            return Expression.Bind(node.Metadata.Member, value);
        }

        private Expression GetTryCatchPropertyExpression(Node node, Expression expression)
        {
            if (this.IsRunningNetFramework() && node.Metadata.Type.IsValueType)
                return expression;

            ParameterExpression ex = Expression.Variable(typeof(Exception));

            MethodInfo constructor = this.GetStaticMethod(typeof(BindingException), nameof(BindingException.FromProperty), typeof(Type), typeof(string), typeof(string), typeof(Exception));

            Expression newException = Expression.Call(constructor, Scope.SchemaType, Expression.Constant(node.Identity.Name), Expression.Default(typeof(string)), ex);
            CatchBlock catchBlock = Expression.Catch(ex, Expression.Throw(newException, expression.Type));

            return Expression.TryCatch(expression, catchBlock);
        }

        private MethodInfo GetStaticMethod(Type type, string methodName, params Type[] arguments)
            => type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.Name == methodName && m.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(arguments));

        private Expression GetOrConditionExpression(IEnumerable<Node> nodes, Func<Node, Expression> condition) => this.GetConditionExpression(nodes, condition, Expression.OrElse);
        private Expression GetAndConditionExpression(IEnumerable<Node> nodes, Func<Node, Expression> condition) => this.GetConditionExpression(nodes, condition, Expression.AndAlso);

        private Expression GetConditionExpression(IEnumerable<Node> nodes, Func<Node, Expression> condition, Func<Expression, Expression, Expression> gateFactory)
        {
            if (!nodes.Any())
                return Expression.Constant(true);

            Expression expr = condition(nodes.First());

            foreach (Node node in nodes.Skip(1))
                expr = gateFactory(expr, condition(node));

            return expr;
        }

        private bool IsRunningNetFramework() => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");


        private class Scope
        {
            public static ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
            public static ParameterExpression Lists { get; } = Expression.Parameter(typeof(ExpandingArray), "lists");
            public static ParameterExpression Scalars { get; } = Expression.Parameter(typeof(ExpandingArray), "scalars");
            public static ParameterExpression Helpers { get; } = Expression.Parameter(typeof(ExpandingArray), "helpers");
            public static ParameterExpression SchemaType { get; } = Expression.Parameter(typeof(Type), "schemaType");

            public List<Expression> Body { get; } = new List<Expression>();
        }
    }
}
