using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Reflection;

namespace Jerrycurl.Extensions.Json.Metadata.Contracts
{
    public class JsonBindingContractResolver : IBindingContractResolver
    {
        public int Priority => 1;

        public JsonSerializerOptions Options { get; }

        private readonly JsonBindingHelperContract helper;

        public JsonBindingContractResolver(JsonSerializerOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            this.helper = new JsonBindingHelperContract(options);
        }

        private MethodInfo GetColumnReaderProxy(IBindingColumnInfo columnInfo)
        {
            if (columnInfo.Column.Type == typeof(string))
                return typeof(IDataRecord).GetMethod(nameof(IDataReader.GetString), new[] { typeof(int) });

            return null;
        }

        private Expression GetValueReaderProxy(IBindingValueInfo valueInfo)
        {
            Expression value = valueInfo.Value;

            if (value.Type != typeof(object) && value.Type != typeof(string))
                throw BindingException.FromMetadata(valueInfo.Metadata, $"Cannot deserialize JSON from type '{value.Type.GetSanitizedName()}'.");

            Expression nullCheck = null;

            if (valueInfo.CanBeDbNull)
                nullCheck = Expression.TypeIs(value, typeof(DBNull));

            if (valueInfo.CanBeNull)
            {
                Expression isNull = Expression.ReferenceEqual(value, Expression.Constant(null, value.Type));

                if (nullCheck == null)
                    nullCheck = isNull;
                else
                    nullCheck = Expression.AndAlso(nullCheck, isNull);
            }

            if (value.Type == typeof(object))
                value = Expression.Convert(value, typeof(string));

            Expression jsonValue = this.GetJsonDeserializer(valueInfo.Metadata, value, valueInfo.Helper);

            if (nullCheck != null)
                return Expression.Condition(nullCheck, Expression.Default(jsonValue.Type), jsonValue);

            return jsonValue;
        }

        private Expression GetJsonDeserializer(IBindingMetadata metadata, Expression value, Expression helper)
        {
            MethodInfo deserializeMethod = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Deserialize), new[] { typeof(string), typeof(Type), typeof(JsonSerializerOptions) });

            if (helper != null)
            {
                Expression methodCall = Expression.Call(deserializeMethod, value, Expression.Constant(metadata.Type), helper);

                return Expression.Convert(methodCall, metadata.Type);
            }
            else
            {
                Expression methodCall = Expression.Call(deserializeMethod, value, Expression.Constant(metadata.Type), Expression.Constant(null));

                return Expression.Convert(methodCall, metadata.Type);
            }
        }

        private bool HasJsonAttribute(IBindingMetadata metadata)
        {
            return metadata.Relation.Annotations.OfType<JsonAttribute>().Any();
        }

        public IBindingParameterContract GetParameterContract(IBindingMetadata metadata)
        {
            if (!this.HasJsonAttribute(metadata))
                return null;

            return new BindingParameterContract()
            {
                Convert = o => o != null ? (object)JsonSerializer.Serialize(o, metadata.Type, this.Options) : DBNull.Value,
            };

        }
        public IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata) => null;
        public IBindingValueContract GetValueContract(IBindingMetadata metadata)
        {
            if (!this.HasJsonAttribute(metadata))
                return null;

            return new BindingValueContract()
            {
                Convert = this.GetValueReaderProxy,
                Read = this.GetColumnReaderProxy,
            };
        }

        public IBindingHelperContract GetHelperContract(IBindingMetadata metadata)
        {
            if (!this.HasJsonAttribute(metadata))
                return null;

            return this.helper;
        }
    }
}
