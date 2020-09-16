using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jerrycurl.Data.Commands.Internal.Caching;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Commands.Internal.Compilation
{
    internal class CommandCompiler
    {
        private delegate void BufferInternalWriter(IDataReader dataReader, FieldBuffer[] buffers, ElasticArray helpers, Type schemaType);
        private delegate void BufferInternalConverter(IField field, object value, ElasticArray helpers, Type schemaType);

        public BufferConverter Compile(MetadataIdentity metadata, ColumnMetadata columnInfo)
        {
            IBindingMetadata binding = metadata.GetMetadata<IBindingMetadata>();

            ParameterExpression inputParam = Expression.Parameter(typeof(object));
            Expression value = inputParam;

            if (metadata != null)
            {
                Type sourceType = null;

                if (columnInfo != null)
                {
                    BindingColumnInfo bindingColumnInfo = new BindingColumnInfo()
                    {
                        Column = columnInfo,
                        CanBeNull = true,
                        Metadata = binding,
                    };

                    sourceType = binding.Value?.Read(bindingColumnInfo)?.ReturnType;
                }

                BindingValueInfo valueInfo = new BindingValueInfo()
                {
                    CanBeNull = true,
                    CanBeDbNull = true,
                    Metadata = binding,
                    Value = value,
                    SourceType = sourceType,
                    TargetType = binding.Type,
                };

                value = binding.Value?.Convert?.Invoke(valueInfo) ?? inputParam;
            }

            value = this.GetObjectExpression(value);

            return Expression.Lambda<BufferConverter>(value, inputParam).Compile();
        }

        public BufferWriter Compile(IEnumerable<ColumnName> columnNames)
        {
            List<Expression> body = new List<Expression>();

            int index = 0;

            foreach (ColumnName columnName in columnNames)
            {
                IBindingMetadata metadata = columnName.Metadata.GetMetadata<IBindingMetadata>();

                Expression value = this.GetValueExpression(metadata, columnName.Column);
                Expression writer = this.GetWriterExpression(index++, value);

                body.Add(writer);
            }

            ParameterExpression[] arguments = new[] { Arguments.DataReader, Arguments.Pipes, Arguments.Helpers, Arguments.SchemaType };
            Expression block = Expression.Block(body);

            BufferInternalWriter writerFunc = Expression.Lambda<BufferInternalWriter>(block, arguments).Compile();

            ElasticArray helpers = new ElasticArray();

            return (dr, ps) => writerFunc(dr, ps, helpers, null);
        }

        private Expression GetValueExpression(IBindingMetadata metadata, ColumnMetadata columnInfo)
        {
            MethodInfo readMethod = this.GetValueReaderMethod(metadata, columnInfo);
            MethodInfo nullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new[] { typeof(int) });

            Expression readIndex = Expression.Constant(columnInfo.Index);
            Expression dataReader = Arguments.DataReader;

            if (readMethod.DeclaringType != typeof(IDataReader) && readMethod.DeclaringType != typeof(IDataRecord))
                dataReader = Expression.Convert(dataReader, readMethod.DeclaringType);

            Expression readValue = Expression.Call(dataReader, readMethod, readIndex);
            Expression isDbNull = Expression.Call(dataReader, nullMethod, readIndex);
            Expression dbNull = Expression.Field(null, typeof(DBNull).GetField(nameof(DBNull.Value)));

            Expression nullObject = Expression.Convert(dbNull, typeof(object));
            Expression readObject = Expression.Convert(readValue, typeof(object));

            return Expression.Condition(isDbNull, nullObject, readObject);
        }

        private Expression GetWriterExpression(int index, Expression value)
        {
            MethodInfo writeMethod = typeof(FieldBuffer).GetMethod(nameof(FieldBuffer.Write));

            Expression pipeIndex = Expression.ArrayAccess(Arguments.Pipes, Expression.Constant(index));

            return Expression.Call(pipeIndex, writeMethod, value);
        }

        private MethodInfo GetValueReaderMethod(IBindingMetadata metadata, ColumnMetadata columnInfo)
        {
            BindingColumnInfo bindingInfo = new BindingColumnInfo()
            {
                Metadata = metadata,
                Column = columnInfo,
            };

            MethodInfo readMethod = metadata.Value?.Read?.Invoke(bindingInfo);

            if (readMethod == null)
                readMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });

            return readMethod;
        }

        private Expression GetObjectExpression(Expression expression)
        {
            if (expression.Type.IsValueType)
                return Expression.Convert(expression, typeof(object));

            return expression;
        }

        private static class Arguments
        {
            public static ParameterExpression DataReader { get; } = Expression.Parameter(typeof(IDataReader), "dataReader");
            public static ParameterExpression Pipes { get; } = Expression.Parameter(typeof(FieldBuffer[]), "pipes");
            public static ParameterExpression Helpers { get; } = Expression.Parameter(typeof(ElasticArray), "helpers");
            public static ParameterExpression SchemaType { get; } = Expression.Parameter(typeof(Type), "schemaType");
        }
    }
}
