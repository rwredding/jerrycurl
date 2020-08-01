using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Extensions;
using Jerrycurl.Data.Queries.Internal.V11.Factories;
using Jerrycurl.Data.Queries.Internal.V11.Parsers;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Reflection;
using System.Diagnostics;

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

            foreach (HelperWriter writer in tree.Helpers)
                scope.Body.Add(this.GetWriterExpression(writer));

            foreach (SlotWriter writer in tree.Slots)
                scope.Body.Add(this.GetWriterExpression(writer));

            // in loop (or not)
            foreach (ListWriter writer in tree.Lists)
                scope.Body.Add(this.GetWriterExpression(writer));

            foreach (AggregateWriter writer in tree.Aggregates)
                scope.Body.Add(this.GetWriterExpression(writer));

            ParameterExpression[] initArgs = new[] { Scope.Slots };
            ParameterExpression[] writeArgs = new[] { Scope.DataReader, Scope.Slots, Scope.Aggregates, Scope.Helpers, Scope.SchemaType };

            Expression initBlock = Expression.Block(scope.Body);
            BufferInternalInitializer init = this.Compile<BufferInternalInitializer>(initBlock, initArgs);

            Expression writeAllBlock = Expression.Block(scope.Body);
            BufferInternalWriter writeAll = this.Compile<BufferInternalWriter>(writeAllBlock, writeArgs);

            Expression writeOneBlock = Expression.Block(scope.Body);
            BufferInternalWriter writeOne = this.Compile<BufferInternalWriter>(writeOneBlock, writeArgs);

            ElasticArray helpers = new ElasticArray();
            Type schemaType = tree.Schema.Model;

            return new BufferWriter()
            {
                WriteAll = (buf, dr) => writeAll(dr, helpers, buf.Slots, buf.Aggregates, schemaType),
                WriteOne = (buf, dr) => writeOne(dr, helpers, buf.Slots, buf.Aggregates, schemaType),
                Initialize = buf => init(buf.Slots),
            };
        }

        public AggregateReader<TItem> Compile<TItem>(AggregateTree tree)
        {
            Scope scope = new Scope();

            scope.Body.Add(this.GetReaderExpression(tree.Aggregate));

            Expression block = Expression.Block(scope.Body);

            ParameterExpression[] arguments = new[] { Scope.Slots, Scope.Aggregates, Scope.SchemaType };
            AggregateInternalReader<TItem> reader = this.Compile<AggregateInternalReader<TItem>>(block, arguments);

            Type schemaType = tree.Schema.Model;

            return buf => reader(buf.Slots, buf.Aggregates, schemaType);
        }

        public EnumerateReader<TItem> Compile<TItem>(EnumerateTree tree)
        {
            Scope scope = new Scope();

            foreach (HelperWriter writer in tree.Helpers)
                scope.Body.Add(this.GetWriterExpression(writer));

            scope.Body.Add(this.GetReaderExpression(tree.Item));

            Expression block = Expression.Block(scope.Body);

            ParameterExpression[] arguments = new[] { Scope.DataReader, Scope.Slots, Scope.Aggregates, Scope.SchemaType };
            EnumerateInternalReader<TItem> reader = this.Compile<EnumerateInternalReader<TItem>>(block, arguments);

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

            return Expression.IfThen(isNotNull, assignBoth);
        }

        private Expression GetWriterExpression(ListWriter writer)
        {
            Expression value = this.GetReaderExpression(writer.Item);
            Expression list = writer.Slot;

            if (writer.JoinKey != null)
            {
                Expression listArray = this.GetDictionaryGetOrAddArrayExpression(writer.Slot, writer.JoinKey.Variable);
                Expression listIndex = this.GetElasticIndexExpression(listArray, writer.BufferIndex);

                list = Expression.Convert(listIndex, writer.Metadata.Composition.Construct.Type);
            }

            Expression addToList = Expression.Call(list, writer.Metadata.Composition.Add, value);

            return this.GetBlockExpression(writer.PrimaryKey, new[] { writer.JoinKey }, addToList);
        }

        private Expression GetBlockExpression(KeyReader primaryKey, IEnumerable<KeyReader> joinKeys, Expression body)
        {
            List<Expression> expressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            foreach (KeyReader joinKey in joinKeys.Concat(new[] { primaryKey }.NotNull()))
            {
                foreach (DataReader valueReader in joinKey.Values)
                {
                    Expression isDbNull = this.GetIsDbNullExpression(valueReader);
                    Expression value = this.GetValueExpression(valueReader);
                    Expression assignNull = Expression.Assign(valueReader.IsDbNull, isDbNull);
                    Expression convertedValue = this.GetConvertExpression(value, valueReader);
                    Expression valueOrDefault = Expression.Condition(assignNull, Expression.Default(convertedValue.Type), convertedValue);
                    Expression assignValue = Expression.Assign(valueReader.Value, valueOrDefault);

                    variables.Add(valueReader.IsDbNull);
                    variables.Add(valueReader.Value);
                    expressions.Add(assignValue);
                }
            }

            expressions.Add(body);

            if (primaryKey != null)
            {
                Expression missingPk = this.GetOrConditionExpression(primaryKey.Values, this.GetIsDbNullExpression);
                Expression block = Expression.Block(variables, expressions);

                return Expression.Condition(missingPk, Expression.Default(body.Type), block);
            }
            else
            {
                expressions.Add(body);

                return Expression.Block(variables, expressions);
            }
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
            Expression value = this.GetReaderOrDbNullExpression(writer.Data);
            Expression convertedValue = this.GetConvertExpression(value, writer.Data);

            if (convertedValue.Type.IsValueType)
                convertedValue = Expression.Convert(convertedValue, typeof(object));

            return Expression.Assign(arrayIndex, convertedValue);
        }

        #endregion

        #region " Readers "

        private Expression GetReaderExpression(NodeReader reader) => reader switch
        {
            DataReader b => this.GetReaderExpression(b, b.IsDbNull, b.Value, b.CanBeDbNull),
            AggregateReader b => this.GetReaderExpression(b, b.IsDbNull, b.Value, canBeDbNull: true),
            NewReader b => this.GetReaderExpression(b),
            ListReader b => this.GetReaderExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetReaderExpression(ListReader reader)
        {
            NewExpression newExpression = reader.Metadata.Composition.Construct;
            Expression arrayIndex = this.GetElasticIndexExpression(reader.Array, reader.BufferIndex);
            Expression assignNew = Expression.Assign(arrayIndex, newExpression);
            Expression noArray = Expression.ReferenceEqual(reader.Array, Expression.Constant(null));
            Expression noList = Expression.ReferenceEqual(arrayIndex, Expression.Constant(null));

            Expression ifNull1 = Expression.Convert(Expression.Condition(noList, assignNew, arrayIndex), reader.Metadata.Type);
            Expression ifNull2 = Expression.Condition(noArray, Expression.Default(reader.Metadata.Type), ifNull1);

            return ifNull2;
        }

        private Expression GetReaderOrDbNullExpression(DataReader reader)
        {
            if (!reader.CanBeDbNull)
                return this.GetReaderExpression(reader, reader.IsDbNull, reader.Value, reader.CanBeDbNull);

            Expression isDbNull = reader.IsDbNull ?? this.GetIsDbNullExpression(reader);
            Expression value = reader.Value ?? this.GetValueExpression(reader);
            Expression dbNull = Expression.Convert(Expression.Constant(DBNull.Value), typeof(object));
            
            return Expression.Condition(isDbNull, dbNull, Expression.Convert(value, typeof(object)));
        }

        private Expression GetReaderExpression(NodeReader reader, Expression isDbNull, Expression value, bool canBeDbNull)
        {
            isDbNull ??= this.GetIsDbNullExpression(reader);
            value ??= this.GetValueExpression(reader);

            if (canBeDbNull)
                return Expression.Condition(isDbNull, Expression.Default(value.Type), value);

            return value;
        }

        private Expression GetReaderExpression(NewReader reader)
        {
            Expression noPrimaryKey = this.GetOrConditionExpression(reader.PrimaryKey.Values, this.GetIsDbNullExpression);
            Expression hasPrimaryKey = noPrimaryKey != null ? Expression.Not(noPrimaryKey) : null;

            NewExpression newExpression = reader.Metadata.Composition.Construct;
            Expression memberInit = Expression.MemberInit(newExpression, reader.Properties.Select(b =>
            {
                if (!b.Metadata.HasFlag(BindingMetadataFlags.Writable))
                    throw BindingException.FromMetadata(b.Metadata, "Cannot bind to read-only property.");

                Expression value = this.GetReaderExpression(b);

                return Expression.Bind(b.Metadata.Member, value);
            }));

            if (hasPrimaryKey != null)
                return Expression.Condition(noPrimaryKey, Expression.Default(reader.Metadata.Type), memberInit);

            return memberInit;
        }

        #endregion

        #region " Lists "
        private Expression GetListExpression(ListWriter writer)
        {
            return null;
        }
        #endregion

        #region  " IsDbNull "

        public Expression GetIsDbNullExpression(NodeReader reader) => reader switch
        {
            DataReader b => this.GetIsDbNullExpression(b),
            AggregateReader b => this.GetIsDbNullExpression(b),
            _ => throw new InvalidOperationException(),
        };

        public Expression GetIsDbNullExpression(DataReader reader)
        {
            MethodInfo isDbNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            return Expression.Call(Scope.DataReader, isDbNullMethod, Expression.Constant(reader.Column.Index));
        }

        public Expression GetIsDbNullExpression(AggregateReader reader)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(Scope.Aggregates, reader.BufferIndex);

            return Expression.TypeIs(arrayIndex, typeof(DBNull));
        }

        #endregion

        #region " Values "
        private Expression GetValueExpression(NodeReader reader) => reader switch
        {
            DataReader b => this.GetValueExpression(b),
            AggregateReader b => this.GetValueExpression(b),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetValueExpression(AggregateReader reader)
        {
            Expression arrayIndex = this.GetElasticIndexExpression(Scope.Aggregates, reader.BufferIndex);

            return Expression.Convert(arrayIndex, reader.Metadata.Type);
        }

        private Expression GetValueExpression(DataReader reader)
        {
            MethodInfo readMethod = this.GetDataReaderMethod(reader);

            Expression index = Expression.Constant(reader.Column.Index);
            Expression dataReader = Scope.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            return Expression.Call(dataReader, readMethod, index);
        }

        private MethodInfo GetDataReaderMethod(DataReader binding)
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

        private Expression GetConvertExpression(Expression value, DataReader reader)
        {
            ParameterExpression variable = Expression.Variable(value.Type);

            BindingValueInfo valueInfo = new BindingValueInfo()
            {
                SourceType = value.Type,
                TargetType = reader.Metadata.Type,
                CanBeNull = !value.Type.IsNotNullableValueType(),
                CanBeDbNull = false,
                Metadata = reader.Metadata,
                Value = variable,
                Helper = reader.Helper,
            };

            Expression convertedValue;

            try
            {
                convertedValue = reader.Metadata.Value?.Convert?.Invoke(valueInfo);
            }
            catch (Exception ex)
            {
                throw BindingException.FromProperty(reader.Metadata.Identity.Name, ex.Message, ex);
            }

            if (convertedValue == null || object.ReferenceEquals(convertedValue, variable))
                return value;

            Expression assignVar = Expression.Assign(variable, value);

            return Expression.Block(new[] { variable }, assignVar, convertedValue);
        }

        #endregion

        #region " Helpers "

        private TDelegate Compile<TDelegate>(Expression block, params ParameterExpression[] arguments)
            => Expression.Lambda<TDelegate>(block, arguments).Compile();

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

        private Expression GetOrConditionExpression(IEnumerable<NodeReader> readers, Func<NodeReader, Expression> condition, Expression emptyValue = null)
            => this.GetConditionExpression(readers, condition, Expression.OrElse, emptyValue);
        private Expression GetAndConditionExpression(IEnumerable<NodeReader> readers, Func<NodeReader, Expression> condition, Expression emptyValue = null)
            => this.GetConditionExpression(readers, condition, Expression.AndAlso, emptyValue);

        private Expression GetConditionExpression(IEnumerable<NodeReader> readers, Func<NodeReader, Expression> condition, Func<Expression, Expression, Expression> gateFactory, Expression emptyValue = null)
        {
            if (!readers.Any())
                return emptyValue;

            Expression expr = condition(readers.First());

            foreach (NodeReader reader in readers.Skip(1))
                expr = gateFactory(expr, condition(reader));

            return expr;
        }

        private bool IsRunningNetFramework() => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

        #endregion

        private class Scope
        {
            public static ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
            public static ParameterExpression Slots { get; } = Expression.Parameter(typeof(ExpandingArray), "slots");
            public static ParameterExpression Aggregates { get; } = Expression.Parameter(typeof(ExpandingArray), "aggregates");
            public static ParameterExpression Helpers { get; } = Expression.Parameter(typeof(ExpandingArray), "helpers");
            public static ParameterExpression SchemaType { get; } = Expression.Parameter(typeof(Type), "schemaType");

            public List<Expression> Body { get; } = new List<Expression>();
        }
    }
}
