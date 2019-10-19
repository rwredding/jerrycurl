using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Builders;
using Jerrycurl.Data.Queries.Internal.Nodes;
using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Linq.Expressions;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class ResultCompiler
    {
        private const bool UseTryCatchExpressions = true;

        public TypeState State { get; }

        private VariableStore variables;

        public ResultCompiler(TypeState state)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public Action<ExpandingArray, ExpandingArray<bool>> CompileAggregate()
        {
            this.variables = new VariableStore();

            AggregateBuilder builder = new AggregateBuilder(this.State);
            AggregateNode aggregateNode = builder.Build();

            ContractValidator.Validate(aggregateNode);

            List<Expression> body = new List<Expression>();

            this.AddAggregateValue(aggregateNode, body);

            var block = this.variables.ToBlock(body);
            var lambda = Expression.Lambda<Action<ExpandingArray, ExpandingArray<bool>, Type>>(block, ResultArgs.Lists,
                                                                                                      ResultArgs.Bits,
                                                                                                      ResultArgs.SchemaType);
            var func = lambda.Compile();

            Type schemaType = this.State.Schema.Model;

            return (lists, bits) => func(lists, bits, schemaType);
        }

        public ResultState<TItem> Compile<TItem>(TableIdentity heading)
        {
            ResultBuilder builder = new ResultBuilder(this.State, heading);
            ResultNode result = builder.Build();

            ContractValidator.Validate(result);

            ResultState<TItem> factory = new ResultState<TItem>()
            {
                Aggregate = this.State.Aggregate,
                Item = this.CompileForEnumerate<TItem>(heading),
                Initializer = this.CompileForInitializer(result),
                List = this.CompileForList(result),
                ListItem = this.CompileForListItem(result),
            };

            return factory;
        }


        private Action<ExpandingArray, ExpandingArray, ExpandingArray<bool>> CompileForInitializer(ResultNode resultNode)
        {
            this.variables = new VariableStore();

            List<Expression> body = new List<Expression>();

            foreach (ListNode listNode in resultNode.Lists)
                this.AddInitializeList(listNode, body);

            if (body.Count == 0)
                return (a, b, c) => { };

            var block = Expression.Block(body);
            var lambda = Expression.Lambda<Action<ExpandingArray, ExpandingArray, ExpandingArray<bool>>>(block, ResultArgs.Lists,
                                                                                                                ResultArgs.Dicts,
                                                                                                                ResultArgs.Bits);
            var func = lambda.Compile();

            return func;
        }

        private Action<IDataReader, ExpandingArray, ExpandingArray> CompileForListItem(ResultNode resultNode)
        {
            this.variables = new VariableStore();

            if (resultNode.Lists.Count == 0)
                return (dr, lists, dicts) => { };

            List<Expression> body = new List<Expression>();

            foreach (ListNode listNode in resultNode.Lists)
                this.AddExistingListVariables(listNode, body);

            this.AddHelperVariables(resultNode.Helpers, body);
            this.AddDataReaderInnerLoopExpression(resultNode, body);

            var block = this.variables.ToBlock(body);
            var lambda = Expression.Lambda<Action<IDataReader, ExpandingArray, ExpandingArray, ExpandingArray, Type>>(block, ResultArgs.DataReader,
                                                                                                                             ResultArgs.Lists,
                                                                                                                             ResultArgs.Dicts,
                                                                                                                             ResultArgs.Helpers,
                                                                                                                             ResultArgs.SchemaType);
            var func = lambda.Compile();

            ExpandingArray helpers = this.GetHelperArray(resultNode.Helpers);
            Type schemaType = this.State.Schema.Model;

            return (dr, lists, dicts) => func(dr, lists, dicts, helpers, schemaType);
        }

        private Action<IDataReader, ExpandingArray, ExpandingArray, ExpandingArray<bool>> CompileForList(ResultNode resultNode)
        {
            this.variables = new VariableStore();

            if (resultNode.Lists.Count == 0)
                return (dr, lists, dicts, bits) => { };

            List<Expression> body = new List<Expression>();

            foreach (ListNode listNode in resultNode.Lists)
                this.AddListVariables(listNode, body);
                
            this.AddHelperVariables(resultNode.Helpers, body);
            this.AddDataReaderLoopExpression(resultNode, body);

            Expression block = this.variables.ToBlock(body);

            var lambda = Expression.Lambda<Action<IDataReader, ExpandingArray, ExpandingArray, ExpandingArray<bool>, ExpandingArray, Type>>(block, ResultArgs.DataReader,
                                                                                                                                                   ResultArgs.Lists,
                                                                                                                                                   ResultArgs.Dicts,
                                                                                                                                                   ResultArgs.Bits,
                                                                                                                                                   ResultArgs.Helpers,
                                                                                                                                                   ResultArgs.SchemaType);
            var func = lambda.Compile();

            ExpandingArray helpers = this.GetHelperArray(resultNode.Helpers);
            Type schemaType = this.State.Schema.Model;

            return (dr, lists, dicts, bits) => func(dr, lists, dicts, bits, helpers, schemaType);
        }

        private Func<IDataReader, TItem> CompileForEnumerate<TItem>(TableIdentity heading)
        {
            this.variables = new VariableStore();

            ItemBuilder builder = new ItemBuilder(this.State.Schema, heading);
            EnumerateNode enumerateNode = builder.Build();
            MetadataNode resultNode = enumerateNode.Items.FirstOrDefault(n => n.HasFlag(NodeFlags.Result));

            if (resultNode == null)
                return r => default;

            ContractValidator.Validate(resultNode);

            List<Expression> body = new List<Expression>();

            this.AddHelperVariables(enumerateNode.Helpers, body);

            body.Add(this.GetNodeExpression(resultNode));

            var block = this.variables.ToBlock(body);
            var lambda = Expression.Lambda<Func<IDataReader, ExpandingArray, Type, TItem>>(block, ResultArgs.DataReader,
                                                                                                  ResultArgs.Helpers,
                                                                                                  ResultArgs.SchemaType);
            var func = lambda.Compile();

            ExpandingArray helpers = this.GetHelperArray(enumerateNode.Helpers);
            Type schemaType = this.State.Schema.Model;

            return dr => func(dr, helpers, schemaType);
        }

        #region " Value binding "

        private Expression GetNodeExpression(MetadataNode node, bool? checkDbNull = null)
        {
            if (node.Column != null)
            {
                checkDbNull ??= !node.HasFlag(NodeFlags.Key);

                Expression value = this.GetValueExpression(node, node.Metadata.Type, checkDbNull.Value);

                return UseTryCatchExpressions ? this.GetTryCatchPropertyExpression(node, value) : value;
            }
            else if (node.HasFlag(NodeFlags.Dynamic))
                return this.GetDynamicExpression(node);
            else if (node.ListIndex != null && node.ElementIndex != null)
                return this.GetArrayIndexedValueExpression(node);
            else
            {
                Expression construct = this.GetConstructExpression(node);

                if (node.NullKey != null && checkDbNull != false)
                    construct = this.GetNullKeyExpression(node, construct);

                return construct;
            }
        }

        private Expression GetConstructExpression(MetadataNode node)
        {
            NewExpression newExpression = node.Metadata.Composition.Construct;

            return Expression.MemberInit(newExpression, node.Properties.Select(this.GetPropertyBinding));
        }

        private Expression GetDynamicExpression(MetadataNode node)
        {
            ParameterExpression variable = Expression.Variable(node.Metadata.Composition.Construct.Type);
            NewExpression newExpression = node.Metadata.Composition.Construct;

            List<Expression> body = new List<Expression>()
            {
                Expression.Assign(variable, newExpression),
            };

            foreach (MetadataNode propertyNode in node.Properties)
            {
                string memberName = this.State.Schema.Notation.Member(propertyNode.Identity.Name);

                Expression member = this.GetNodeExpression(propertyNode);

                if (member.Type.IsValueType)
                    member = Expression.Convert(member, typeof(object));

                Expression addCall = Expression.Call(variable, node.Metadata.Composition.AddDynamic, Expression.Constant(memberName), member);

                body.Add(addCall);
            }

            body.Add(variable);

            return Expression.Block(new[] { variable }, body);
        }

        private MemberAssignment GetPropertyBinding(MetadataNode node)
        {
            if (!node.Metadata.HasFlag(BindingMetadataFlags.Writable))
                throw BindingException.FromMetadata(node.Metadata, "Cannot bind to read-only property.");

            Expression expression = this.GetNodeExpression(node);

            return Expression.Bind(node.Metadata.Member, expression);
        }


        private Expression GetRawValueExpression(MetadataNode node)
        {
            MethodInfo readMethod = this.GetValueReadMethod(node);

            Expression index = Expression.Constant(node.Column.Index);
            Expression dataReader = ResultArgs.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            return Expression.Call(dataReader, readMethod, index);
        }

        private Expression GetRawValueNullableExpression(MetadataNode node, Expression value, Type targetType)
        {
            Expression isDbNull = this.GetIsDbNullStoredExpression(node);

            if (this.IsNotNullableValueType(value.Type) && (Nullable.GetUnderlyingType(targetType) != null || targetType == typeof(object)))
            {
                Type nullableType = typeof(Nullable<>).MakeGenericType(value.Type);

                return Expression.Condition(isDbNull, Expression.Default(nullableType), Expression.Convert(value, nullableType));
            }
            else
                return Expression.Condition(isDbNull, Expression.Default(value.Type), value);
        }

        private Expression GetValueConvertExpression(Expression value, IBindingMetadata metadata, Type sourceType, Type targetType, bool canBeNull, int? helperIndex)
        {
            ParameterExpression variable = Expression.Variable(value.Type);
            ParameterExpression helper = helperIndex != null ? this.variables.Get($"helper{helperIndex}") : null;

            BindingValueInfo valueInfo = new BindingValueInfo()
            {
                SourceType = sourceType,
                TargetType = targetType,
                CanBeNull = (canBeNull && !this.IsNotNullableValueType(value.Type)),
                CanBeDbNull = false,
                Metadata = metadata,
                Value = variable,
                Helper = helper,
            };

            Expression convertedValue;

            try
            {
                convertedValue = metadata.Value?.Convert?.Invoke(valueInfo);
            }
            catch (Exception ex)
            {
                throw BindingException.FromProperty(metadata.Identity.Name, ex.Message, ex);
            }

            if (convertedValue == null || object.ReferenceEquals(convertedValue, variable))
                return value;

            Expression assignVar = Expression.Assign(variable, value);

            return Expression.Block(new[] { variable }, assignVar, convertedValue);
        }

        private Expression GetValueExpression(MetadataNode node, Type targetType, bool checkDbNull)
        {
            string keyName = $"key{node.Column.Index}";

            if (this.variables.HasVariable(keyName))
            {
                Expression rawValue = this.variables.Get(keyName);
                Expression nullableValue = (checkDbNull && rawValue.Type != targetType) ? this.GetRawValueNullableExpression(node, rawValue, targetType) : rawValue;

                return this.GetValueConvertExpression(nullableValue, node.Metadata, rawValue.Type, targetType, checkDbNull, node.Helper?.Index);
            }
            else
            {
                Expression rawValue = this.GetRawValueExpression(node);
                Expression nullableValue = checkDbNull ? this.GetRawValueNullableExpression(node, rawValue, targetType) : rawValue;

                return this.GetValueConvertExpression(nullableValue, node.Metadata, rawValue.Type, targetType, checkDbNull, node.Helper?.Index);
            }
        }

        private Expression GetArrayIndexedValueExpression(MetadataNode node)
        {
            if (node.Metadata.HasFlag(BindingMetadataFlags.List))
            {
                Expression array = this.variables.Get($"joins{node.ListIndex.Value}");
                Expression arrayIndex = this.GetArrayIndexExpression(array, node.ElementIndex.Value);
                Expression newElement = node.Metadata.Composition.Construct;
                Expression noArray = this.GetIsNullExpression(array);
                Expression noValue = this.GetIsNullExpression(arrayIndex);
                Expression assignNew = Expression.Assign(arrayIndex, newElement);
                Expression indexIf = Expression.Condition(noValue, assignNew, arrayIndex);

                return Expression.Condition(noArray, Expression.Default(newElement.Type), Expression.Convert(indexIf, newElement.Type));
            }
            else
            {
                Expression array = this.variables.Get("joins" + node.ListIndex.Value);
                Expression arrayIndex = this.GetArrayIndexExpression(array, node.ElementIndex.Value);
                Expression isNull = this.GetIsNullExpression(array);

                return Expression.Condition(isNull, Expression.Default(node.Metadata.Type), Expression.Convert(arrayIndex, node.Metadata.Type));
            }
        }

        private Expression GetTryCatchKeyExpression(KeyNode node, Expression expression)
        {
            if (this.IsRunningNetFramework())
                return expression;

            ParameterExpression ex = Expression.Variable(typeof(Exception));

            MethodInfo constructor = this.GetStaticMethod(typeof(BindingException), nameof(BindingException.FromReference), typeof(Type), typeof(string), typeof(string), typeof(string), typeof(Exception));

            Expression newException = Expression.Call(constructor, ResultArgs.SchemaType, Expression.Constant(node.Reference.Metadata.Identity.Name),
                                                      Expression.Constant(node.Reference.Other.Metadata.Identity.Name), Expression.Default(typeof(string)), ex);
            CatchBlock catchBlock = Expression.Catch(ex, Expression.Throw(newException, expression.Type));

            return Expression.TryCatch(expression, catchBlock);
        }

        private Expression GetTryCatchPropertyExpression(MetadataNode node, Expression expression)
        {
            if (this.IsRunningNetFramework() && node.Metadata.Type.IsValueType)
                return expression;

            ParameterExpression ex = Expression.Variable(typeof(Exception));

            MethodInfo constructor = this.GetStaticMethod(typeof(BindingException), nameof(BindingException.FromProperty), typeof(Type), typeof(string), typeof(string), typeof(Exception));

            Expression newException = Expression.Call(constructor, ResultArgs.SchemaType, Expression.Constant(node.Identity.Name), Expression.Default(typeof(string)), ex);
            CatchBlock catchBlock = Expression.Catch(ex, Expression.Throw(newException, expression.Type));

            return Expression.TryCatch(expression, catchBlock);
        }

        private bool IsRunningNetFramework() => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

        private MethodInfo GetValueReadMethod(MetadataNode valueNode)
        {
            BindingColumnInfo bindingInfo = new BindingColumnInfo()
            {
                Metadata = valueNode.Metadata,
                Column = valueNode.Column,
            };

            MethodInfo readMethod = valueNode.Metadata.Value?.Read?.Invoke(bindingInfo);

            if (readMethod == null)
                readMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });

            return readMethod;
        }

        #endregion

        private Expression GetNullKeyExpression(MetadataNode node, Expression body)
        {
            Expression nullTest = this.GetOrConditionExpression(node.NullKey.Value, this.GetIsDbNullExpression);

            return Expression.Condition(nullTest, Expression.Default(body.Type), body);
        }

        private Expression GetIsDbNullStoredExpression(MetadataNode node) => this.variables.Get($"null{node.Column.Index}") ?? this.GetIsDbNullExpression(node);

        private Expression GetIsDbNullExpression(MetadataNode node)
        {
            MethodInfo isDbNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            Expression isDbNull = Expression.Call(ResultArgs.DataReader, isDbNullMethod, Expression.Constant(node.Column.Index));

            return isDbNull;
        }

        private Expression GetArrayGetOrSetExpression(Expression arrayIndex, Expression newElement)
        {
            Expression isNull = Expression.ReferenceEqual(arrayIndex, Expression.Constant(null));
            Expression assignNew = Expression.Assign(arrayIndex, newElement);
            Expression ifNull = Expression.Condition(isNull, assignNew, arrayIndex);

            return Expression.Convert(ifNull, newElement.Type);
        }

        #region " Reflection "

        private Expression GetArrayIndexExpression(Expression arrayExpression, int index)
        {
            PropertyInfo indexer = arrayExpression.Type.GetProperty("Item");

            return Expression.Property(arrayExpression, indexer, Expression.Constant(index));
        }

        private Expression GetDictionaryAddExpression(Expression dictionary, Expression key, Expression value)
        {
            MethodInfo addMethod = dictionary.Type.GetMethod("Add");

            return Expression.Call(dictionary, addMethod, key, value);
        }

        private Expression GetDictionaryTryGetExpression(Expression dictionary, Expression key, Expression outVariable)
        {
            MethodInfo tryGetMethod = dictionary.Type.GetMethod("TryGetValue");

            return Expression.Call(dictionary, tryGetMethod, key, outVariable);
        }

        private bool IsNotNullableValueType(Type type) => (type.IsValueType && Nullable.GetUnderlyingType(type) == null);

        private MethodInfo GetStaticMethod(Type type, string methodName, params Type[] arguments)
            => type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.Name == methodName && m.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(arguments));

        private Expression GetOrConditionExpression(IEnumerable<MetadataNode> nodes, Func<MetadataNode, Expression> condition) => this.GetConditionExpression(nodes, condition, Expression.OrElse);
        private Expression GetAndConditionExpression(IEnumerable<MetadataNode> nodes, Func<MetadataNode, Expression> condition) => this.GetConditionExpression(nodes, condition, Expression.AndAlso);

        private Expression GetConditionExpression(IEnumerable<MetadataNode> nodes, Func<MetadataNode, Expression> condition, Func<Expression, Expression, Expression> gateFactory)
        {
            if (!nodes.Any())
                return Expression.Constant(true);

            Expression expr = condition(nodes.First());

            foreach (MetadataNode node in nodes.Skip(1))
                expr = gateFactory(expr, condition(node));

            return expr;
        }


        private Type GetDictionaryType(IEnumerable<Type> keyType)
        {
            IEnumerable<Type> nonNullable = keyType.Select(t => Nullable.GetUnderlyingType(t) ?? t);

            Type compositeType = this.GetCompositeKeyType(nonNullable);

            return typeof(Dictionary<,>).MakeGenericType(compositeType, typeof(ExpandingArray));
        }

        private Expression GetDictionaryGetOrAddArrayExpression(Expression dictionary, KeyNode keyNode, ParameterExpression arrayVariable, ParameterExpression keyVariable)
        {
            IEnumerable<Expression> keyValues = keyNode.Value.Zip(keyNode.Type).Select(t =>
            {
                Expression value = this.GetValueExpression(t.l, t.r, false);

                return this.GetTryCatchKeyExpression(keyNode, value);
            });

            Expression key = this.GetCompositeKeyExpression(keyValues);
            Expression setKey = Expression.Assign(keyVariable, key);
            Expression tryGet = this.GetDictionaryTryGetExpression(dictionary, setKey, arrayVariable);
            Expression hasNoKey = Expression.Not(tryGet);
            Expression newArray = Expression.New(typeof(ExpandingArray));
            Expression setArray = Expression.Assign(arrayVariable, newArray);
            Expression addArray = this.GetDictionaryAddExpression(dictionary, keyVariable, setArray);

            return Expression.IfThen(hasNoKey, addArray);
        }


        private Expression GetIsNullExpression(Expression value) => Expression.ReferenceEqual(value, Expression.Constant(null));

        #endregion

        #region " Composite key "

        private Expression GetCompositeKeyExpression(IEnumerable<Expression> values)
        {
            Type compositeType = this.GetCompositeKeyType(values.Select(e => e.Type));

            Expression[] valueArray = values.ToArray();

            if (valueArray.Length <= 4)
            {
                ConstructorInfo constructor = compositeType.GetConstructors()[0];

                return Expression.New(constructor, valueArray.Take(4));
            }
            else
            {
                ConstructorInfo constructor = compositeType.GetConstructors()[0];
                Expression rest = this.GetCompositeKeyExpression(values.Skip(4));

                return Expression.New(constructor, valueArray[0], valueArray[1], valueArray[2], valueArray[3], rest);
            }
        }

        private Type GetCompositeKeyType(IEnumerable<Type> keyType)
        {
            Type[] typeArray = keyType.ToArray();

            if (typeArray.Length == 0)
                return null;
            else if (typeArray.Length == 1)
                return typeof(CompositeKey<>).MakeGenericType(typeArray[0]);
            else if (typeArray.Length == 2)
                return typeof(CompositeKey<,>).MakeGenericType(typeArray[0], typeArray[1]);
            else if (typeArray.Length == 3)
                return typeof(CompositeKey<,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2]);
            else if (typeArray.Length == 4)
                return typeof(CompositeKey<,,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2], typeArray[3]);
            else
            {
                Type restType = this.GetCompositeKeyType(keyType.Skip(4));

                return typeof(CompositeKey<,,,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2], typeArray[3], restType);
            }
        }

        #endregion

        private void AddDataReaderInnerLoopExpression(ResultNode resultNode, List<Expression> body)
        {
            this.AddKeyVariables(resultNode.Elements, body);

            foreach (ListNode listNode in resultNode.Lists)
                this.AddJoinVariables(listNode, body);

            foreach (ElementNode listNode in resultNode.Elements)
                this.AddValues(listNode, body);
        }

        private void AddDataReaderLoopExpression(ResultNode resultNode, List<Expression> body)
        {
            LabelTarget label = Expression.Label();

            List<Expression> loopBody = new List<Expression>();

            this.AddDataReaderInnerLoopExpression(resultNode, loopBody);

            Expression read = Expression.Call(ResultArgs.DataReader, typeof(IDataReader).GetMethod(nameof(IDataReader.Read)));
            Expression ifRead = Expression.IfThenElse(read, Expression.Block(loopBody), Expression.Break(label));
            Expression loop = Expression.Loop(ifRead, label);

            body.Add(loop);
        }

        private void AddKeyVariables(IEnumerable<ElementNode> elementNodes, List<Expression> body)
        {
            IList<KeyNode> keyNodes = elementNodes.SelectMany(n => n.Keys).ToList();

            foreach (MetadataNode node in keyNodes.SelectMany(k => k.Value).GroupBy(n => n.Column.Index).Select(g => g.First()))
            {
                string nullName = $"null{node.Column.Index}";
                string rawName = $"key{node.Column.Index}";

                Expression rawValue = this.GetRawValueExpression(node);

                ParameterExpression nullVariable = this.variables.Add($"null{node.Column.Index}", typeof(bool));
                ParameterExpression keyVariable = this.variables.Add(rawName, rawValue.Type);

                Expression isDbNull = this.GetIsDbNullExpression(node);
                Expression setNull = Expression.Assign(nullVariable, isDbNull);
                Expression defaultValue = Expression.Default(rawValue.Type);
                Expression keyValue = Expression.Condition(setNull, defaultValue, rawValue);
                Expression setKey = Expression.Assign(keyVariable, keyValue);

                body.Add(setKey);
            }
        }


        private ExpandingArray GetHelperArray(IEnumerable<HelperNode> helperNodes)
        {
            ExpandingArray helpers = new ExpandingArray();

            foreach (HelperNode helperNode in helperNodes)
                helpers[helperNode.Index] = helperNode.Object;

            return helpers;
        }

        private void AddHelperVariables(IEnumerable<HelperNode> nodes, List<Expression> body)
        {
            foreach (HelperNode node in nodes)
            {
                ParameterExpression variable = this.variables.Add($"helper{node.Index}", node.Type);

                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Helpers, node.Index);
                Expression castValue = Expression.Convert(arrayIndex, variable.Type);
                Expression assignVar = Expression.Assign(variable, castValue);

                body.Add(assignVar);
            }
        }

        private void AddInitializeList(ListNode node, List<Expression> body)
        {
            Expression bitsIndex = this.GetArrayIndexExpression(ResultArgs.Bits, node.Index.Value);

            body.Add(Expression.Assign(bitsIndex, Expression.Constant(true)));

            if (node.KeyType != null)
            {
                Type dictionaryType = this.GetDictionaryType(node.KeyType);

                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Dicts, node.Index.Value);
                Expression newDict = Expression.New(dictionaryType);
                Expression isNull = this.GetIsNullExpression(arrayIndex);
                Expression ifBlock = Expression.IfThen(isNull, Expression.Assign(arrayIndex, newDict));

                body.Add(ifBlock);
            }
            else if (node.Metadata.HasFlag(BindingMetadataFlags.Item))
            {
                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Lists, node.Index.Value);
                Expression newList = node.Metadata.Parent.Composition.Construct;
                Expression isNull = this.GetIsNullExpression(arrayIndex);
                Expression ifBlock = Expression.IfThen(isNull, Expression.Assign(arrayIndex, newList));

                body.Add(ifBlock);
            }
        }

        private void AddExistingListVariables(ListNode node, List<Expression> body)
        {
            if (node.KeyType != null)
            {
                Type dictionaryType = this.GetDictionaryType(node.KeyType);

                ParameterExpression variable = this.variables.Add("dicts" + node.Index.Value, dictionaryType);

                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Dicts, node.Index.Value);
                Expression castValue = Expression.Convert(arrayIndex, dictionaryType);
                Expression assignVar = Expression.Assign(variable, castValue);

                body.Add(assignVar);
            }
            else if (node.Metadata.HasFlag(BindingMetadataFlags.Item))
            {
                Type listType = node.Metadata.Parent.Composition.Construct.Type;

                ParameterExpression variable = this.variables.Add("lists" + node.Index, listType);

                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Lists, node.Index.Value);
                Expression castValue = Expression.Convert(arrayIndex, listType);
                Expression assignVar = Expression.Assign(variable, castValue);

                body.Add(assignVar);
            }
        }

        private void AddListVariables(ListNode node, List<Expression> body)
        {
            Expression bitsIndex = this.GetArrayIndexExpression(ResultArgs.Bits, node.Index.Value);

            body.Add(Expression.Assign(bitsIndex, Expression.Constant(true)));

            if (node.KeyType != null)
            {
                Type dictionaryType = this.GetDictionaryType(node.KeyType);

                ParameterExpression variable = this.variables.Add("dicts" + node.Index.Value, dictionaryType);

                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Dicts, node.Index.Value);
                Expression newDict = Expression.New(variable.Type);
                Expression getOrSet = this.GetArrayGetOrSetExpression(arrayIndex, newDict);
                Expression assignVar = Expression.Assign(variable, getOrSet);

                body.Add(assignVar);
            }
            else if (node.Metadata.HasFlag(BindingMetadataFlags.Item))
            {
                IBindingMetadata listMetadata = node.Metadata.Parent;

                ParameterExpression variable = this.variables.Add("lists" + node.Index, listMetadata.Composition.Construct.Type);

                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Lists, node.Index.Value);
                Expression newElement = listMetadata.Composition.Construct;
                Expression getOrSet = this.GetArrayGetOrSetExpression(arrayIndex, newElement);
                Expression assignVar = Expression.Assign(variable, getOrSet);

                body.Add(assignVar);
            }
        }

        private void AddJoinVariables(ListNode node, List<Expression> body)
        {
            if (node.ParentKey != null)
            {
                Type compositeType = this.GetCompositeKeyType(node.ParentKey.Type);

                ParameterExpression keyVariable = Expression.Variable(compositeType, "key");
                ParameterExpression arrayVariable = this.variables.Add("joins" + node.ParentKey.ParentIndex, typeof(ExpandingArray));

                Expression dictionary = this.variables.Get("dicts" + node.ParentKey.ParentIndex);
                Expression getOrAdd = this.GetDictionaryGetOrAddArrayExpression(dictionary, node.ParentKey, arrayVariable, keyVariable);
                Expression keyIsNull = this.GetOrConditionExpression(node.ParentKey.Value, this.GetIsDbNullStoredExpression);
                Expression setDefault = Expression.Assign(arrayVariable, Expression.Default(arrayVariable.Type));
                Expression ifNull = Expression.IfThenElse(keyIsNull, setDefault, getOrAdd);
                Expression ifBlock = Expression.Block(new[] { keyVariable }, ifNull);

                body.Add(ifBlock);
            }
        }

        private void AddValues(ElementNode node, List<Expression> body)
        {
            bool checkDbNull = (!node.ChildKeys.Any() && node.Value.NullKey == null);

            Expression value = this.GetNodeExpression(node.Value, checkDbNull);

            if (node.ChildKeys.Any())
            {
                ParameterExpression valueVariable = Expression.Variable(value.Type, "item");

                List<Expression> dictBody = new List<Expression>()
                {
                    Expression.Assign(valueVariable, value),
                };

                foreach (KeyNode childKey in node.ChildKeys)
                {
                    Type keyType = this.GetCompositeKeyType(childKey.Type);

                    ParameterExpression keyVariable = Expression.Variable(keyType, "key");
                    ParameterExpression arrayVariable = Expression.Variable(typeof(ExpandingArray), "array");

                    Expression dictionary = this.variables.Get("dicts" + childKey.ParentIndex);
                    Expression getOrAdd = this.GetDictionaryGetOrAddArrayExpression(dictionary, childKey, arrayVariable, keyVariable);
                    Expression keyIsNull = this.GetOrConditionExpression(childKey.Value, this.GetIsDbNullStoredExpression);
                    Expression arrayIndex = this.GetArrayIndexExpression(arrayVariable, childKey.ChildIndex.Value);

                    if (node.List != null)
                    {
                        ParameterExpression listVariable = Expression.Variable(node.List.Composition.Construct.Type, "list");

                        Expression castIndex = Expression.Convert(arrayIndex, listVariable.Type);
                        Expression list = Expression.Assign(listVariable, castIndex);
                        Expression isNull = this.GetIsNullExpression(list);
                        Expression newList = Expression.Assign(listVariable, node.List.Composition.Construct);
                        Expression setArray = Expression.Assign(arrayIndex, newList);
                        Expression ifNull = Expression.IfThen(isNull, setArray);
                        Expression addToList = Expression.Call(listVariable, node.List.Composition.Add, valueVariable);
                        Expression block = Expression.Block(new[] { keyVariable, arrayVariable, listVariable, }, getOrAdd, ifNull, addToList);
                        Expression keyBody = Expression.IfThen(Expression.Not(keyIsNull), block);

                        dictBody.Add(keyBody);
                    }
                    else
                    {
                        Expression assign = Expression.Assign(arrayIndex, valueVariable);
                        Expression block = Expression.Block(new[] { keyVariable, arrayVariable, }, getOrAdd, assign);
                        Expression keyBody = Expression.IfThen(Expression.Not(keyIsNull), block);

                        dictBody.Add(keyBody);
                    }
                }

                Expression hasNoKeys = this.GetOrConditionExpression(node.ChildKeys.SelectMany(k => k.Value).Distinct(), this.GetIsDbNullStoredExpression);
                Expression hasAnyKey = Expression.Not(hasNoKeys);
                Expression fullBlock = Expression.Block(new[] { valueVariable }, dictBody);
                Expression fullIf = Expression.IfThen(hasAnyKey, fullBlock);

                body.Add(fullIf);
            }
            else if (node.List != null)
            {
                MethodInfo addMethod = node.List.Composition.Add;

                Expression list = this.variables.Get("lists" + node.ListIndex.Value);
                Expression addToList = Expression.Call(list, addMethod, value);

                if (node.Value.NullKey != null)
                {
                    Expression isNull = this.GetOrConditionExpression(node.Value.NullKey.Value, this.GetIsDbNullStoredExpression);
                    Expression ifNotNull = Expression.IfThen(Expression.Not(isNull), addToList);

                    body.Add(ifNotNull);
                }
                else
                    body.Add(addToList);
            }
            else
            {
                Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Lists, node.ListIndex.Value);
                Expression nullCheck = this.GetIsNullExpression(arrayIndex);
                Expression setValue = Expression.Assign(arrayIndex, Expression.Convert(value, typeof(object)));

                if (node.Value.NullKey != null)
                {
                    Expression isNull = this.GetOrConditionExpression(node.Value.NullKey.Value, this.GetIsDbNullStoredExpression);

                    nullCheck = Expression.AndAlso(nullCheck, Expression.Not(isNull));
                }

                Expression ifNull = Expression.IfThen(nullCheck, setValue);

                body.Add(ifNull);
            }
        }

        private void AddAggregateValue(AggregateNode aggrNode, List<Expression> body)
        {
            static MetadataNode[] createState(MetadataNode node) => node.Tree().Where(n => n.ListIndex != null).ToArray();

            Expression memberBuilder(MetadataNode node, MetadataNode[] listState)
            {
                if (node.ListIndex != null)
                {
                    Expression arrayIndex = this.GetArrayIndexExpression(ResultArgs.Lists, node.ListIndex.Value);
                    Expression convertedValue = Expression.Convert(arrayIndex, node.Metadata.Type);

                    if (this.IsNotNullableValueType(node.Metadata.Type))
                    {
                        Expression isNull = this.GetIsNullExpression(arrayIndex);

                        return Expression.Condition(isNull, Expression.Default(node.Metadata.Type), convertedValue);
                    }

                    return convertedValue;
                }

                MetadataNode[] newState = createState(node);
                List<MemberAssignment> bindings = new List<MemberAssignment>();

                foreach (MetadataNode propertyNode in node.Properties)
                {
                    MemberAssignment memberBind = Expression.Bind(propertyNode.Metadata.Member, memberBuilder(propertyNode, newState));

                    bindings.Add(memberBind);
                }

                Expression value = Expression.MemberInit(node.Metadata.Composition.Construct, bindings);

                if (!listState.SequenceEqual(newState) && newState.Length > 0)
                {
                    Expression isNull = this.GetAndConditionExpression(newState, n =>
                    {
                        Expression bitIndex = this.GetArrayIndexExpression(ResultArgs.Bits, n.ListIndex.Value);

                        return Expression.Not(bitIndex);
                    });

                    value = Expression.Condition(isNull, Expression.Default(value.Type), value);
                }

                return value;
            }

            ParameterExpression listVariable = this.variables.Add("list", aggrNode.Metadata.Composition.Construct.Type);

            Expression arrayIndex2 = this.GetArrayIndexExpression(ResultArgs.Lists, aggrNode.Index);
            Expression newList = Expression.Assign(listVariable, aggrNode.Metadata.Composition.Construct);
            Expression setList = Expression.Assign(arrayIndex2, newList);

            body.Add(setList);

            if (aggrNode.Item != null)
            {
                MetadataNode[] initState = createState(aggrNode.Item);

                Expression hasValue = this.GetOrConditionExpression(initState, n => this.GetArrayIndexExpression(ResultArgs.Bits, n.ListIndex.Value));

                Expression itemValue = memberBuilder(aggrNode.Item, initState);
                Expression addItem = Expression.Call(listVariable, aggrNode.Metadata.Composition.Add, itemValue);
                Expression ifBlock = Expression.IfThen(hasValue, addItem);

                body.Add(ifBlock);
            }
        }
    }
}
