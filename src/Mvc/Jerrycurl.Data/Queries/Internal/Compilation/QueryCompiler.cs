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
using Jerrycurl.Data.Queries.Internal.Parsing;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Reflection;
using Jerrycurl.Data.Queries.Internal.Binding;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;
using Jerrycurl.Data.Queries.Internal.Caching;
using System.Net.Http.Headers;

namespace Jerrycurl.Data.Queries.Internal.Compilation
{
    internal class QueryCompiler
    {
        private delegate void BufferInternalWriter(IDataReader dataReader, ElasticArray slots, ElasticArray aggregates, ElasticArray helpers, Type schemaType);
        private delegate void BufferInternalInitializer(ElasticArray slots);
        private delegate TItem AggregateInternalReader<TItem>(ElasticArray slots, ElasticArray aggregates, Type schemaType);
        private delegate TItem EnumerateInternalReader<TItem>(IDataReader dataReader, ElasticArray helpers, Type schemaType);

        private BufferWriter CompileBuffer(BufferTree tree, Expression initialize, Expression writeOne, Expression writeAll)
        {
            ParameterExpression[] initArgs = new[] { Arguments.Slots };
            ParameterExpression[] writeArgs = new[] { Arguments.DataReader, Arguments.Slots, Arguments.Aggregates, Arguments.Helpers, Arguments.SchemaType };

            BufferInternalInitializer initializeFunc = this.Compile<BufferInternalInitializer>(initialize, initArgs);
            BufferInternalWriter writeOneFunc = this.Compile<BufferInternalWriter>(writeOne, writeArgs);
            BufferInternalWriter writeAllFunc = this.Compile<BufferInternalWriter>(writeAll, writeArgs);

            ElasticArray helpers = this.GetHelperArray(tree.Helpers);
            Type schemaType = tree.Schema.Model;

            if (tree.QueryType == QueryType.Aggregate && tree.AggregateNames.Any())
            {
                AggregateName[] names = tree.AggregateNames.ToArray();

                return new BufferWriter()
                {
                    Initialize = buf =>
                    {
                        buf.Aggregate.Names.AddRange(names);

                        initializeFunc(buf.Slots);
                    },
                    WriteOne = (buf, dr) => writeOneFunc(dr, buf.Slots, buf.Aggregate.Values, helpers, schemaType),
                    WriteAll = (buf, dr) =>
                    {
                        buf.Aggregate.Names.AddRange(names);

                        writeAllFunc(dr, buf.Slots, buf.Aggregate.Values, helpers, schemaType);
                    },
                };
            }
            else
            {
                return new BufferWriter()
                {
                    WriteAll = (buf, dr) => writeAllFunc(dr, buf.Slots, buf.Aggregate.Values, helpers, schemaType),
                    WriteOne = (buf, dr) => writeOneFunc(dr, buf.Slots, buf.Aggregate.Values, helpers, schemaType),
                    Initialize = buf => initializeFunc(buf.Slots),
                };
            }
        }

        public BufferWriter Compile(BufferTree tree)
        {
            List<ParameterExpression> variables = new List<ParameterExpression>();

            List<Expression> initList = new List<Expression>();
            List<Expression> oneList = new List<Expression>();
            List<Expression> allList = new List<Expression>();

            List<Expression> body = new List<Expression>();

            foreach (SlotWriter writer in tree.Slots)
            {
                Expression initExpression = this.GetInitializeExpression(writer);
                Expression writeExpresssion = this.GetWriterExpression(writer);

                initList.Add(writeExpresssion);
                oneList.Add(initExpression);
                allList.Add(writeExpresssion);
                allList.Add(initExpression);

                variables.Add(writer.Variable);
            }

            foreach (HelperWriter writer in tree.Helpers)
            {
                Expression writeExpression = this.GetWriterExpression(writer);

                oneList.Add(writeExpression);
                allList.Add(writeExpression);

                variables.Add(writer.Variable);
            }

            foreach (AggregateWriter writer in tree.Aggregates)
                body.Add(this.GetWriterExpression(writer));

            foreach (ListWriter writer in tree.Lists.OrderBy(w => w.Priority))
                body.Add(this.GetWriterExpression(writer));

            oneList.AddRange(body);
            allList.Add(this.GetDataReaderLoopExpression(body));

            Expression initialize = this.GetBlockOrExpression(initList);
            Expression writeOne = this.GetBlockOrExpression(oneList, variables);
            Expression writeAll = this.GetBlockOrExpression(allList, variables);

            return this.CompileBuffer(tree, initialize, writeOne, writeAll);
        }

        public AggregateReader<TItem> Compile<TItem>(AggregateTree tree)
        {
            Expression block = this.GetBinderExpression(tree.Aggregate);

            ParameterExpression[] arguments = new[] { Arguments.Slots, Arguments.Aggregates, Arguments.SchemaType };
            AggregateInternalReader<TItem> reader = this.Compile<AggregateInternalReader<TItem>>(block, arguments);

            Type schemaType = tree.Schema.Model;

            return buf => reader(buf.Slots, buf.Aggregate.Values, schemaType);
        }

        public EnumerateReader<TItem> Compile<TItem>(EnumerateTree tree)
        {
            List<Expression> body = new List<Expression>();

            foreach (HelperWriter writer in tree.Helpers)
                body.Add(this.GetWriterExpression(writer));

            body.Add(this.GetBinderExpression(tree.Item));

            ParameterExpression[] arguments = new[] { Arguments.DataReader, Arguments.Helpers, Arguments.SchemaType };
            EnumerateInternalReader<TItem> reader = this.Compile<EnumerateInternalReader<TItem>>(body, arguments);

            ElasticArray helpers = this.GetHelperArray(tree.Helpers);
            Type schemaType = tree.Schema.Model;

            return dr => reader(dr, helpers, schemaType);
        }

        #region " Initialize "
        private Expression GetInitializeExpression(SlotWriter writer)
        {
            Expression slotIndex = this.GetElasticIndexExpression(Arguments.Slots, writer.BufferIndex);
            Expression convertIndex = Expression.Convert(slotIndex, writer.Variable.Type);

            return Expression.Assign(writer.Variable, convertIndex);
        }

        #endregion

        #region " Writers "
        private Expression GetWriterExpression(SlotWriter writer)
        {
            Expression slotIndex = this.GetElasticIndexExpression(Arguments.Slots, writer.BufferIndex);
            NewExpression newList;

            if (writer.KeyType == null)
                newList = writer.Metadata.Composition.Construct;
            else
            {
                Type dictionaryType = this.GetDictionaryType(writer.KeyType);

                newList = Expression.New(dictionaryType);
            }

            Expression assignNew = Expression.Assign(slotIndex, newList);
            Expression isNull = Expression.ReferenceEqual(slotIndex, Expression.Constant(null));

            return Expression.IfThen(isNull, assignNew);
        }

        private Expression GetWriterExpression(ListWriter writer)
        {
            Expression value = this.GetBinderExpression(writer.Item);
            Expression writeItem;

            if (writer.JoinKey == null)
                writeItem = Expression.Call(writer.Slot, writer.Metadata.Composition.Add, value);
            else
            {
                Expression arrayIndex = this.GetElasticIndexExpression(writer.JoinKey.Array, writer.BufferIndex);

                if (writer.IsOneToMany)
                    writeItem = Expression.Assign(arrayIndex, value);
                else
                {
                    Expression list = Expression.Convert(arrayIndex, writer.Metadata.Composition.Construct.Type);

                    writeItem = Expression.Call(list, writer.Metadata.Composition.Add, value);
                }
            }

            return this.GetKeyBlockExpression(writer.PrimaryKey, new[] { writer.JoinKey }.NotNull(), writeItem);
        }

        
        private Expression GetWriterExpression(HelperWriter writer)
        {
            Expression helperIndex = this.GetElasticIndexExpression(Arguments.Helpers, writer.BufferIndex);
            Expression castValue = Expression.Convert(helperIndex, writer.Variable.Type);

            return Expression.Assign(writer.Variable, castValue);
        }

        private Expression GetWriterExpression(AggregateWriter writer)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(Arguments.Aggregates, writer.BufferIndex);
            Expression value = this.GetBinderOrNullExpression(writer.Data);

            return Expression.Assign(arrayIndex, value);
        }

        #endregion

        #region " Keys "
        private Expression GetKeyInitValueExpression(ValueBinder binder)
        {
            Expression value = this.GetValueExpression(binder);
            Expression convertedValue = this.GetConvertExpression(value, binder);

            if (binder.CanBeDbNull)
            {
                Expression isDbNull = this.GetIsDbNullExpression(binder);
                Expression assignNull = Expression.Assign(binder.IsDbNull, isDbNull);

                convertedValue = Expression.Condition(assignNull, Expression.Default(convertedValue.Type), convertedValue);
            }

            return Expression.Assign(binder.Variable, convertedValue);
        }

        private Expression GetKeyInitArrayExpression(KeyBinder binder)
        {
            IEnumerable<ParameterExpression> isNullVars = binder.Values.Where(v => v.CanBeDbNull).Select(v => v.IsDbNull).ToList();

            Expression newKey = this.GetNewCompositeKeyExpression(binder);
            Expression setKey = Expression.Assign(binder.Variable, newKey);
            Expression tryGet = this.GetDictionaryTryGetExpression(binder.Slot, setKey, binder.Array);
            Expression newArray = Expression.New(typeof(ElasticArray));
            Expression setArray = Expression.Assign(binder.Array, newArray);
            Expression addArray = this.GetDictionaryAddExpression(binder.Slot, binder.Variable, setArray);
            Expression getOrAdd = Expression.IfThenElse(tryGet, Expression.Default(typeof(void)), addArray);

            if (isNullVars.Any())
            {
                Expression notNull = Expression.Not(this.GetOrConditionExpression(isNullVars));

                getOrAdd = Expression.IfThen(notNull, getOrAdd);
            }

            return getOrAdd;
        }

        private Expression GetKeyBlockExpression(KeyBinder primaryKey, IEnumerable<KeyBinder> joinKeys, Expression body)
        {
            List<Expression> expressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            IList<ValueBinder> allValues = joinKeys.SelectMany(k => k.Values).ToList();

            foreach (ValueBinder valueBinder in allValues.Distinct())
            {
                expressions.Add(this.GetKeyInitValueExpression(valueBinder));

                if (valueBinder.CanBeDbNull)
                    variables.Add(valueBinder.IsDbNull);

                variables.Add(valueBinder.Variable);
                
            }

            foreach (KeyBinder joinBinder in joinKeys)
            {
                expressions.Add(this.GetKeyInitArrayExpression(joinBinder));

                variables.Add(joinBinder.Variable);
                variables.Add(joinBinder.Array);                
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
            ColumnBinder b => this.GetBinderExpression(b, b.IsDbNull, b.Variable, b.CanBeDbNull),
            AggregateBinder b => this.GetBinderExpression(b, b.IsDbNull, b.Variable, b.CanBeDbNull),
            NewBinder b => this.GetBinderExpression(b),
            JoinBinder b => this.GetBinderExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetBinderExpression(JoinBinder binder)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(binder.Array, binder.ArrayIndex);
            Expression hasArray = Expression.ReferenceNotEqual(binder.Array, Expression.Constant(null));

            if (binder.IsManyToOne)
            {
                Expression ifArray = Expression.Condition(hasArray, arrayIndex, Expression.Constant(null));

                return Expression.Convert(ifArray, binder.Metadata.Type);
            }
            else
            {
                Expression newExpression = binder.Metadata.Composition.Construct;
                Expression assignNew = Expression.Assign(arrayIndex, newExpression);
                Expression hasNoList = Expression.ReferenceEqual(arrayIndex, Expression.Constant(null));
                Expression oldOrNewList = Expression.Condition(hasNoList, assignNew, arrayIndex);
                Expression ifArray = Expression.Condition(hasArray, oldOrNewList, Expression.Constant(null));

                return Expression.Convert(ifArray, binder.Metadata.Type);
            }
        }

        private Expression GetBinderOrNullExpression(ColumnBinder binder)
        {
            if (!binder.CanBeDbNull)
                return this.GetBinderExpression(binder, binder.IsDbNull, binder.Variable, binder.CanBeDbNull);

            Expression isDbNull = binder.IsDbNull ?? this.GetIsDbNullExpression(binder);
            Expression nullValue = Expression.Constant(null);
            Expression value = binder.Variable;

            if (value == null)
            {
                value = this.GetValueExpression(binder);
                value = this.GetConvertExpression(value, binder);
            }

            return Expression.Condition(isDbNull, nullValue, Expression.Convert(value, typeof(object)));
        }

        private Expression GetBinderExpression(ValueBinder binder, Expression isDbNull, Expression value, bool canBeDbNull)
        {
            isDbNull ??= this.GetIsDbNullExpression(binder);

            if (value == null)
            {
                value = this.GetValueExpression(binder);
                value = this.GetConvertExpression(value, binder);
            }

            if (canBeDbNull)
                return Expression.Condition(isDbNull, Expression.Default(binder.Metadata.Type), value);

            return value;
        }

        private Expression GetBinderExpression(NewBinder binder)
        {
            if (binder.IsDynamic)
                return this.GetDynamicExpression(binder);

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

        #region " Dynamic "
        private Expression GetDynamicExpression(NewBinder binder)
        {
            ParameterExpression variable = Expression.Variable(binder.Metadata.Composition.Construct.Type);
            NewExpression newExpression = binder.Metadata.Composition.Construct;

            List<Expression> body = new List<Expression>()
            {
                Expression.Assign(variable, newExpression),
            };

            foreach (NodeBinder propertyBinder in binder.Properties)
            {
                string propertyName = propertyBinder.Identity.Schema.Notation.Member(propertyBinder.Identity.Name);

                Expression propertyValue = this.GetBinderExpression(propertyBinder);
                Expression objectValue = propertyValue.Type.IsValueType ? Expression.Convert(propertyValue, typeof(object)) : propertyValue;
                Expression addDynamic = Expression.Call(variable, binder.Metadata.Composition.AddDynamic, Expression.Constant(propertyName), objectValue);

                body.Add(addDynamic);
            }

            body.Add(variable);

            return Expression.Block(new[] { variable }, body);
        }

        #endregion

        #region  " IsDbNull "

        private Expression GetIsDbNullExpression(ValueBinder binder) => binder switch
        {
            ColumnBinder b => this.GetIsDbNullExpression(b),
            AggregateBinder b => this.GetIsDbNullExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetIsDbNullExpression(ColumnBinder reader)
        {
            MethodInfo isNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            return Expression.Call(Arguments.DataReader, isNullMethod, Expression.Constant(reader.Column.Index));
        }

        private Expression GetIsDbNullExpression(AggregateBinder binder)
        {
            Expression array = binder.IsPrincipal ? Arguments.Slots : Arguments.Aggregates;
            Expression arrayIndex = this.GetElasticIndexExpression(array, binder.BufferIndex);

            return Expression.ReferenceEqual(arrayIndex, Expression.Constant(null));
        }

        #endregion

        #region " Values "
        private Expression GetValueExpression(ValueBinder binder) => binder switch
        {
            ColumnBinder b => this.GetValueExpression(b),
            AggregateBinder b => this.GetValueExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetValueExpression(AggregateBinder binder)
        {
            Expression array = binder.IsPrincipal ? Arguments.Slots : Arguments.Aggregates;

            return this.GetElasticIndexExpression(array, binder.BufferIndex);
        }   

        private Expression GetValueExpression(ColumnBinder binder)
        {
            MethodInfo readMethod = this.GetValueReaderMethod(binder);

            Expression index = Expression.Constant(binder.Column.Index);
            Expression dataReader = Arguments.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            return Expression.Call(dataReader, readMethod, index);
        }

        private MethodInfo GetValueReaderMethod(ColumnBinder binder)
        {
            BindingColumnInfo bindingInfo = new BindingColumnInfo()
            {
                Metadata = binder.Metadata,
                Column = binder.Column,
            };

            MethodInfo readMethod = binder.Metadata.Value?.Read?.Invoke(bindingInfo);

            if (readMethod == null)
                readMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });

            return readMethod;
        }

        #endregion

        #region " Convert "
        private Expression GetConvertExpression(Expression value, ValueBinder binder) => binder switch
        {
            AggregateBinder b => this.GetConvertExpression(value, b),
            ColumnBinder b => this.GetConvertExpression(value, b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetConvertExpression(Expression value, AggregateBinder binder)
        {
            Type targetType = binder.KeyType ?? binder.Metadata.Type;

            if (value.Type != targetType)
                return Expression.Convert(value, targetType);

            return value;
        }

        private Expression GetConvertExpression(Expression value, ColumnBinder binder)
        {
            Type targetType = binder.KeyType ?? binder.Metadata.Type;
            ParameterExpression variable = Expression.Variable(value.Type);

            BindingValueInfo valueInfo = new BindingValueInfo()
            {
                SourceType = value.Type,
                TargetType = targetType,
                CanBeNull = (!value.Type.IsValueType || Nullable.GetUnderlyingType(value.Type) != null),
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

        private TDelegate Compile<TDelegate>(IList<Expression> body, IList<ParameterExpression> arguments)
        {
            Expression block = this.GetBlockOrExpression(body);

            return Expression.Lambda<TDelegate>(block, arguments).Compile();
        }
        private TDelegate Compile<TDelegate>(Expression block, params ParameterExpression[] arguments)
            => this.Compile<TDelegate>(new[] { block }, arguments);

        private Expression GetDataReaderLoopExpression(IList<Expression> body)
        {
            LabelTarget label = Expression.Label();

            Expression read = Expression.Call(Arguments.DataReader, typeof(IDataReader).GetMethod(nameof(IDataReader.Read)));
            Expression ifRead = Expression.IfThenElse(read, this.GetBlockOrExpression(body), Expression.Break(label));

            return Expression.Loop(ifRead, label);
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

        private Expression GetBlockOrExpression(IList<Expression> expressions, IList<ParameterExpression> variables = null)
        {
            if (expressions.Count == 1 && (variables == null || !variables.Any()))
                return expressions[0];
            else if (variables == null)
                return Expression.Block(expressions);
            else
                return Expression.Block(variables.NotNull(), expressions);
        }

        private Expression GetNewCompositeKeyExpression(KeyBinder key)
        {
            ConstructorInfo ctor = key.KeyType.GetConstructors()[0];

            return Expression.New(ctor, key.Values.Select(v => v.Variable));
        }

        private ElasticArray GetHelperArray(IEnumerable<HelperWriter> writers)
        {
            ElasticArray array = new ElasticArray();

            foreach (HelperWriter writer in writers)
                array[writer.BufferIndex] = writer.Object;

            return array;
        }

        private Expression GetTryCatchPropertyExpression(NodeBinder binder, Expression propertyExpression)
        {
            if (this.IsRunningNetFramework() && binder.Metadata.Type.IsValueType)
                return propertyExpression;

            ParameterExpression ex = Expression.Variable(typeof(Exception));

            MethodInfo constructor = typeof(BindingException).GetStaticMethod(nameof(BindingException.FromProperty), typeof(Type), typeof(string), typeof(string), typeof(Exception));

            Expression newException = Expression.Call(constructor, Arguments.SchemaType, Expression.Constant(binder.Identity.Name), Expression.Default(typeof(string)), ex);
            CatchBlock catchBlock = Expression.Catch(ex, Expression.Throw(newException, propertyExpression.Type));

            return Expression.TryCatch(propertyExpression, catchBlock);
        }

        private Type GetDictionaryType(Type keyType)
            => typeof(Dictionary<,>).MakeGenericType(keyType, typeof(ElasticArray));

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
            if (conditions == null || !conditions.Any())
                return emptyValue;

            Expression expr = conditions.First();

            foreach (Expression condition in conditions.Skip(1))
                expr = gateFactory(expr, condition);

            return expr;
        }

        private bool IsRunningNetFramework() => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

        #endregion

        private static class Arguments
        {
            public static ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
            public static ParameterExpression Slots { get; } = Expression.Parameter(typeof(ElasticArray), "slots");
            public static ParameterExpression Aggregates { get; } = Expression.Parameter(typeof(ElasticArray), "aggregates");
            public static ParameterExpression Helpers { get; } = Expression.Parameter(typeof(ElasticArray), "helpers");
            public static ParameterExpression SchemaType { get; } = Expression.Parameter(typeof(Type), "schemaType");
        }
    }
}
