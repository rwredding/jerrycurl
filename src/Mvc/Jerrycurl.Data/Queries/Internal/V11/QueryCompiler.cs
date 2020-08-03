using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Extensions;
using Jerrycurl.Data.Queries.Internal.V11.Factories;
using Jerrycurl.Data.Queries.Internal.V11.Parsers;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Reflection;
using Jerrycurl.Data.Queries.Internal.V11.Binding;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryCompiler
    {
        private delegate void BufferInternalWriter(IDataReader dataReader, ElasticArray slots, ElasticArray aggregates, ElasticArray helpers, Type schemaType);
        private delegate void BufferInternalInitializer(ElasticArray slots);
        private delegate TItem AggregateInternalReader<TItem>(ElasticArray slots, ElasticArray aggregates, Type schemaType);
        private delegate TItem EnumerateInternalReader<TItem>(IDataReader dataReader, ElasticArray helpers, Type schemaType);

        public BufferWriter Compile(BufferTree tree)
        {
            Scope scope = new Scope();

            List<Expression> initBody = new List<Expression>();
            List<Expression> body = new List<Expression>();

            foreach (SlotWriter writer in tree.Slots)
                initBody.Add(this.GetWriterExpression(writer));

            foreach (HelperWriter writer in tree.Helpers)
                body.Add(this.GetWriterExpression(writer));

            foreach (AggregateWriter writer in tree.Aggregates)
                body.Add(this.GetWriterExpression(writer));

            foreach (ListWriter writer in tree.Lists)
                body.Add(this.GetWriterExpression(writer));

            List<Expression> loopBody = initBody.Concat(new[] { this.GetDataReaderLoopExpression(body) }).ToList();

            ParameterExpression[] initArgs = new[] { Scope.Slots };
            ParameterExpression[] writeArgs = new[] { Scope.DataReader, Scope.Slots, Scope.Aggregates, Scope.Helpers, Scope.SchemaType };

            BufferInternalInitializer init = this.Compile<BufferInternalInitializer>(initBody, initArgs);
            BufferInternalWriter writeAll = this.Compile<BufferInternalWriter>(loopBody, writeArgs);
            BufferInternalWriter writeOne = this.Compile<BufferInternalWriter>(body, writeArgs);

            ElasticArray helpers = this.GetHelperArray(tree.Helpers);
            Type schemaType = tree.Schema.Model;

            return new BufferWriter()
            {
                WriteAll = (buf, dr) => writeAll(dr, buf.Slots, buf.Aggregates, helpers, schemaType),
                WriteOne = (buf, dr) => writeOne(dr, buf.Slots, buf.Aggregates, helpers, schemaType),
                Initialize = buf => init(buf.Slots),
            };
        }

        public AggregateReader<TItem> Compile<TItem>(AggregateTree tree)
        {
            Scope scope = new Scope();

            scope.Body.Add(this.GetBinderExpression(tree.Aggregate));

            Expression block = Expression.Block(scope.Body);

            ParameterExpression[] arguments = new[] { Scope.Slots, Scope.Aggregates, Scope.SchemaType };
            AggregateInternalReader<TItem> reader = this.Compile<AggregateInternalReader<TItem>>(block, arguments);

            Type schemaType = tree.Schema.Model;

            return buf => reader(buf.Slots, buf.Aggregates, schemaType);
        }

        public EnumerateReader<TItem> Compile<TItem>(EnumerateTree tree)
        {
            List<Expression> body = new List<Expression>();

            foreach (HelperWriter writer in tree.Helpers)
                body.Add(this.GetWriterExpression(writer));

            body.Add(this.GetBinderExpression(tree.Item));

            ParameterExpression[] arguments = new[] { Scope.DataReader, Scope.Helpers, Scope.SchemaType };
            EnumerateInternalReader<TItem> reader = this.Compile<EnumerateInternalReader<TItem>>(body, arguments);

            ElasticArray helpers = this.GetHelperArray(tree.Helpers);
            Type schemaType = tree.Schema.Model;

            return dr => reader(dr, helpers, schemaType);
        }

        #region " Writers "
        private Expression GetWriterExpression(SlotWriter writer)
        {
            Expression slotIndex = this.GetElasticIndexExpression(Scope.Slots, writer.BufferIndex);
            NewExpression newList;

            if (writer.Key == null)
                newList = writer.Metadata.Composition.Construct;
            else
            {
                Type dictionaryType = this.GetDictionaryType(writer.Key);

                newList = Expression.New(dictionaryType);
            }

            Expression oldList = Expression.Convert(slotIndex, newList.Type);
            Expression assignVar = Expression.Assign(writer.Variable, oldList);
            Expression isNotNull = Expression.ReferenceNotEqual(assignVar, Expression.Constant(null));
            Expression assignNew = Expression.Assign(writer.Variable, newList);
            Expression assignBoth = Expression.Assign(slotIndex, assignNew);

            return Expression.Block(new[] { writer.Variable }, Expression.IfThen(isNotNull, assignBoth));
        }

        private Expression GetWriterExpression(ListWriter writer)
        {
            Expression value = this.GetBinderExpression(writer.Item);
            Expression list = writer.Slot;

            if (writer.JoinKey != null)
            {
                Expression listArray = this.GetDictionaryGetOrAddArrayExpression(writer.Slot, writer.JoinKey.Variable);
                Expression listIndex = this.GetElasticIndexExpression(listArray, writer.BufferIndex);

                list = Expression.Convert(listIndex, writer.Metadata.Composition.Construct.Type);
            }

            Expression addToList = Expression.Call(list, writer.Metadata.Composition.Add, value);

            return this.GetKeyBlockExpression(writer.PrimaryKey, new[] { writer.JoinKey }.NotNull(), addToList);
        }

        
        private Expression GetWriterExpression(HelperWriter writer)
        {
            Expression helperIndex = this.GetElasticIndexExpression(Scope.Helpers, writer.BufferIndex);
            Expression castValue = Expression.Convert(helperIndex, writer.Variable.Type);

            return Expression.Assign(writer.Variable, castValue);
        }

        private Expression GetWriterExpression(AggregateWriter writer)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(Scope.Aggregates, writer.BufferIndex);
            Expression value = this.GetBinderOrDbNullExpression(writer.Data);

            return Expression.Assign(arrayIndex, value);
        }

        #endregion

        #region " Keys "
        private Expression GetKeyInitializeExpression(ValueBinder binder)
        {
            Expression isDbNull = this.GetIsDbNullExpression(binder);
            Expression assignNull = Expression.Assign(binder.IsDbNull, isDbNull);
            Expression value = this.GetValueExpression(binder);
            Expression convertedValue = this.GetConvertExpression(value, binder);
            Expression valueOrDefault = Expression.Condition(assignNull, Expression.Default(convertedValue.Type), convertedValue);

            return Expression.Assign(binder.Value, valueOrDefault);
        }

        private Expression GetKeyBlockExpression(KeyBinder primaryKey, IEnumerable<KeyBinder> joinKeys, Expression body)
        {
            List<Expression> expressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            IList<ValueBinder> allValues = joinKeys.SelectMany(k => k.Values).ToList();

            foreach (ValueBinder valueBinder in allValues)
            {
                Expression initializeValue = this.GetKeyInitializeExpression(valueBinder);

                variables.Add(valueBinder.IsDbNull);
                variables.Add(valueBinder.Value);
                expressions.Add(initializeValue);
            }

            foreach (KeyBinder joinBinder in joinKeys)
            {
                IList<ParameterExpression> isNullVars = joinBinder.Values.Where(v => v.CanBeDbNull).Select(v => v.IsDbNull).ToList();

                Expression isMissingKey = this.GetAndConditionExpression(isNullVars);
                Expression key = this.GetCompositeKeyExpression(joinBinder.Values.Select(v => v.Value));
                Expression array = this.GetDictionaryGetOrAddArrayExpression(joinBinder.Slot, key);
                Expression initializeKey = this.GetConditionExpression(isMissingKey, Expression.Constant(null), array);

                variables.Add(joinBinder.Variable);
                expressions.Add(initializeKey);
            }

            expressions.Add(body);

            Expression block = this.GetBlockOrExpression(expressions, variables);

            if (primaryKey != null)
            {
                Expression isMissingKey = this.GetOrConditionExpression(primaryKey.Values, this.GetIsDbNullExpression);

                return Expression.Condition(isMissingKey, Expression.Default(block.Type), block);
            }

            return block;
        }
        #endregion

        #region " Binders "

        private Expression GetBinderExpression(NodeBinder binder) => binder switch
        {
            DataBinder b => this.GetBinderExpression(b, b.IsDbNull, b.Value, b.CanBeDbNull),
            AggregateBinder b => this.GetBinderExpression(b, b.IsDbNull, b.Value, canBeDbNull: true),
            NewBinder b => this.GetBinderExpression(b),
            JoinBinder b => this.GetBinderExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetBinderExpression(JoinBinder binder)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(binder.Array, binder.BufferIndex);

            if (binder.IsOneValue)
                return Expression.Convert(arrayIndex, binder.Metadata.Type);
            else
            {
                NewExpression newExpression = binder.Metadata.Composition.Construct;
                Expression assignNew = Expression.Assign(arrayIndex, newExpression);
                Expression isMissingList = Expression.ReferenceEqual(arrayIndex, Expression.Constant(null));
                Expression oldOrNewList = Expression.Condition(isMissingList, assignNew, arrayIndex);

                return Expression.Convert(oldOrNewList, binder.Metadata.Type);
            }
        }

        private Expression GetBinderOrDbNullExpression(DataBinder binder)
        {
            if (!binder.CanBeDbNull)
                return this.GetBinderExpression(binder, binder.IsDbNull, binder.Value, binder.CanBeDbNull);

            Expression isDbNull = binder.IsDbNull ?? this.GetIsDbNullExpression(binder);
            Expression value = binder.Value ?? this.GetValueExpression(binder);
            Expression dbNull = Expression.Convert(Expression.Constant(DBNull.Value), typeof(object));
            
            return Expression.Condition(isDbNull, dbNull, Expression.Convert(this.GetConvertExpression(value, binder), typeof(object)));
        }

        private Expression GetBinderExpression(ValueBinder binder, Expression isDbNull, Expression value, bool canBeDbNull)
        {
            isDbNull ??= this.GetIsDbNullExpression(binder);
            value ??= this.GetValueExpression(binder);

            if (canBeDbNull)
                return Expression.Condition(isDbNull, Expression.Default(value.Type), value);

            return value;
        }

        private Expression GetBinderExpression(NewBinder binder)
        {
            NewExpression newExpression = binder.Metadata.Composition.Construct;
            Expression memberInit = Expression.MemberInit(newExpression, binder.Properties.Select(b =>
            {
                if (!b.Metadata.HasFlag(BindingMetadataFlags.Writable))
                    throw BindingException.FromMetadata(b.Metadata, "Cannot bind to read-only property.");

                Expression value = this.GetBinderExpression(b);

                return Expression.Bind(b.Metadata.Member, value);
            }));

            return this.GetKeyBlockExpression(binder.PrimaryKey, binder.JoinKeys, memberInit);
        }

        #endregion

        #region  " IsDbNull "

        public Expression GetIsDbNullExpression(ValueBinder binder) => binder switch
        {
            DataBinder b => this.GetIsDbNullExpression(b),
            AggregateBinder b => this.GetIsDbNullExpression(b),
            _ => throw new InvalidOperationException(),
        };

        public Expression GetIsDbNullExpression(DataBinder reader)
        {
            MethodInfo isDbNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            return Expression.Call(Scope.DataReader, isDbNullMethod, Expression.Constant(reader.Column.Index));
        }

        public Expression GetIsDbNullExpression(AggregateBinder reader)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(Scope.Aggregates, reader.BufferIndex);

            return Expression.TypeIs(arrayIndex, typeof(DBNull));
        }

        #endregion

        #region " Values "
        private Expression GetValueExpression(ValueBinder reader) => reader switch
        {
            DataBinder b => this.GetValueExpression(b),
            AggregateBinder b => this.GetValueExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetValueExpression(AggregateBinder reader)
            => this.GetElasticIndexExpression(Scope.Aggregates, reader.BufferIndex);

        private Expression GetValueExpression(DataBinder reader)
        {
            MethodInfo readMethod = this.GetValueReaderMethod(reader);

            Expression index = Expression.Constant(reader.Column.Index);
            Expression dataReader = Scope.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            return Expression.Call(dataReader, readMethod, index);
        }

        private MethodInfo GetValueReaderMethod(DataBinder binding)
        {
            BindingColumnInfo bindingInfo = new BindingColumnInfo()
            {
                Metadata = binding.Metadata,
                Column = binding.Column,
            };

            MethodInfo readMethod = binding.Metadata.Value?.Read?.Invoke(bindingInfo);

            if (readMethod == null)
                readMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });

            return readMethod;
        }

        #endregion

        #region " Convert "
        private Expression GetConvertExpression(Expression value, ValueBinder binder) => binder switch
        {
            AggregateBinder b => this.GetConvertExpression(value, b),
            DataBinder b => this.GetConvertExpression(value, b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetConvertExpression(Expression value, AggregateBinder binder)
        {
            if (value.Type != binder.Metadata.Type)
                return Expression.Convert(value, binder.Metadata.Type);

            return value;
        }

        private Expression GetConvertExpression(Expression value, DataBinder binder)
        {
            ParameterExpression variable = Expression.Variable(value.Type);

            BindingValueInfo valueInfo = new BindingValueInfo()
            {
                SourceType = value.Type,
                TargetType = binder.Metadata.Type,
                CanBeNull = !value.Type.IsNotNullableValueType(),
                CanBeDbNull = false,
                Metadata = binder.Metadata,
                Value = variable,
                Helper = binder.Helper,
            };

            Expression convertedValue;

            try
            {
                convertedValue = binder.Metadata.Value?.Convert?.Invoke(valueInfo);
            }
            catch (Exception ex)
            {
                throw BindingException.FromProperty(binder.Metadata.Identity.Name, ex.Message, ex);
            }

            if (convertedValue == null || object.ReferenceEquals(convertedValue, variable))
                return value;

            Expression assignVar = Expression.Assign(variable, value);

            return Expression.Block(new[] { variable }, assignVar, convertedValue);
        }

        #endregion

        #region " Helpers "

        private Expression GetDataReaderLoopExpression(IList<Expression> body)
        {
            LabelTarget label = Expression.Label();

            Expression read = Expression.Call(Scope.DataReader, typeof(IDataReader).GetMethod(nameof(IDataReader.Read)));
            Expression ifRead = Expression.IfThenElse(read, this.GetBlockOrExpression(body), Expression.Break(label));

            return Expression.Loop(ifRead, label);
        }

        private TDelegate Compile<TDelegate>(IList<Expression> body, IList<ParameterExpression> arguments)
        {
            Expression block = body.Count == 1 ? body[0] : Expression.Block(body);

            return Expression.Lambda<TDelegate>(block, arguments).Compile();
        }
        private TDelegate Compile<TDelegate>(Expression block, params ParameterExpression[] arguments)
            => this.Compile<TDelegate>(new[] { block }, arguments);

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

        private Expression GetDictionaryGetOrAddArrayExpression(Expression dictionary, Expression key)
        {
            Expression arrayVariable = Expression.Variable(typeof(ElasticArray));
            Expression tryGet = this.GetDictionaryTryGetExpression(dictionary, key, arrayVariable);
            Expression hasNoKey = Expression.Not(tryGet);
            Expression newArray = Expression.New(typeof(ElasticArray));
            Expression setArray = Expression.Assign(arrayVariable, newArray);
            Expression addArray = this.GetDictionaryAddExpression(dictionary, key, setArray);

            return Expression.IfThen(hasNoKey, addArray);
        }

        private Expression GetBlockOrExpression(IList<Expression> expressions, IList<ParameterExpression> variables = null)
        {
            if (expressions.Count == 1 && (variables == null || !variables.Any()))
                return expressions[0];
            else if (variables == null)
                return Expression.Block(expressions);
            else
                return Expression.Block(variables, expressions);
        }

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

        private ElasticArray GetHelperArray(IEnumerable<HelperWriter> writers)
        {
            ElasticArray array = new ElasticArray();

            foreach (HelperWriter writer in writers)
                array[writer.BufferIndex] = writer.Object;

            return array;
        }

        private Expression GetTryCatchPropertyExpression(Node node, Expression propertyExpression)
        {
            if (this.IsRunningNetFramework() && node.Metadata.Type.IsValueType)
                return propertyExpression;

            ParameterExpression ex = Expression.Variable(typeof(Exception));

            MethodInfo constructor = typeof(BindingException).GetStaticMethod(nameof(BindingException.FromProperty), typeof(Type), typeof(string), typeof(string), typeof(Exception));

            Expression newException = Expression.Call(constructor, Scope.SchemaType, Expression.Constant(node.Identity.Name), Expression.Default(typeof(string)), ex);
            CatchBlock catchBlock = Expression.Catch(ex, Expression.Throw(newException, propertyExpression.Type));

            return Expression.TryCatch(propertyExpression, catchBlock);
        }

        private Type GetDictionaryType(IEnumerable<Type> keyType)
            => typeof(Dictionary<,>).MakeGenericType(this.GetCompositeKeyType(keyType), typeof(ElasticArray));

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

        private Expression GetElasticIndexExpression(Expression arrayExpression, int index)
        {
            PropertyInfo indexer = arrayExpression.Type.GetProperty("Item");

            return Expression.Property(arrayExpression, indexer, Expression.Constant(index));
        }

        private Expression GetOrConditionExpression<T>(IEnumerable<T> values, Func<T, Expression> condition, Expression emptyValue = null)
            => this.GetConditionExpression(values, condition, Expression.OrElse, emptyValue);

        private Expression GetAndConditionExpression<T>(IEnumerable<T> values, Func<T, Expression> condition, Expression emptyValue = null)
            => this.GetConditionExpression(values, condition, Expression.AndAlso, emptyValue);

        private Expression GetOrConditionExpression(IEnumerable<Expression> conditions, Expression emptyValue = null)
            => this.GetConditionExpression(conditions, Expression.OrElse, emptyValue);

        private Expression GetAndConditionExpression(IEnumerable<Expression> conditions, Expression emptyValue = null)
            => this.GetConditionExpression(conditions, Expression.AndAlso, emptyValue);

        private Expression GetConditionExpression<T>(IEnumerable<T> values, Func<T, Expression> condition, Func<Expression, Expression, Expression> gateFactory, Expression emptyValue = null)
            => this.GetConditionExpression(values.Select(condition), gateFactory, emptyValue);

        private Expression GetConditionExpression(IEnumerable<Expression> conditions, Func<Expression, Expression, Expression> gateFactory, Expression emptyValue = null)
        {
            if (!conditions.Any())
                return emptyValue;

            Expression expr = conditions.First();

            foreach (Expression condition in conditions.Skip(1))
                expr = gateFactory(expr, condition);

            return expr;
        }

        private Expression GetConditionExpression(Expression test, Expression ifTrue, Expression ifFalse)
            => test != null ? Expression.Condition(test, ifTrue, ifFalse) : ifTrue;

        private bool IsRunningNetFramework() => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

        #endregion

        private class Scope
        {
            public static ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
            public static ParameterExpression Slots { get; } = Expression.Parameter(typeof(ElasticArray), "slots");
            public static ParameterExpression Aggregates { get; } = Expression.Parameter(typeof(ElasticArray), "aggregates");
            public static ParameterExpression Helpers { get; } = Expression.Parameter(typeof(ElasticArray), "helpers");
            public static ParameterExpression SchemaType { get; } = Expression.Parameter(typeof(Type), "schemaType");

            public List<Expression> Body { get; } = new List<Expression>();
        }
    }
}
