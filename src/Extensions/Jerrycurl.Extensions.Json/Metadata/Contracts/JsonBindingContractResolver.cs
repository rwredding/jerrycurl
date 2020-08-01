using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
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

                nullCheck = nullCheck == null ? isNull : Expression.AndAlso(nullCheck, isNull);
            }

            if (value.Type == typeof(object))
                value = Expression.Convert(value, typeof(string));

            Expression targetValue;

            if (valueInfo.TargetType == typeof(JsonDocument))
                targetValue = this.GetParseDocumentExpression(valueInfo.Metadata, value);
            else if (valueInfo.TargetType == typeof(JsonElement))
                targetValue = this.GetParseElementExpression(valueInfo.Metadata, value);
            else
                targetValue = this.GetDeserializeExpression(valueInfo.Metadata, value, valueInfo.Helper);

            if (nullCheck != null)
                return Expression.Condition(nullCheck, Expression.Default(targetValue.Type), targetValue);

            return targetValue;
        }

        private Expression GetParseElementExpression(IBindingMetadata metadata, Expression value)
        {
            Expression document = this.GetParseDocumentExpression(metadata, value);

            return Expression.Property(document, "RootElement");
        }

        private Expression GetParseDocumentExpression(IBindingMetadata metadata, Expression value)
        {
            MethodInfo parseMethod = typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), new[] { typeof(string), typeof(JsonDocumentOptions) });

            return Expression.Call(parseMethod, value, Expression.Default(typeof(JsonDocumentOptions)));
        }

        private Expression GetDeserializeExpression(IBindingMetadata metadata, Expression value, Expression helper)
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

        private bool HasJsonAttribute(IBindingMetadata metadata) => metadata.Relation.Annotations.OfType<JsonAttribute>().Any();
        private bool IsNativeJsonDocument(IBindingMetadata metadata) => (metadata.Type == typeof(JsonElement));
        private bool IsNativeJsonElement(IBindingMetadata metadata) => (metadata.Type == typeof(JsonDocument));
        private bool IsNativeJsonType(IBindingMetadata metadata) => (this.IsNativeJsonDocument(metadata) || this.IsNativeJsonElement(metadata));

        public IBindingParameterContract GetParameterContract(IBindingMetadata metadata)
        {
            if (this.IsNativeJsonDocument(metadata))
            {
                return new BindingParameterContract()
                {
                    Convert = o => o != null ? (object)JsonSerializer.Serialize(((JsonDocument)o).RootElement, typeof(JsonElement), this.Options) : DBNull.Value,
                };
            }
            else if (this.HasJsonAttribute(metadata) || this.IsNativeJsonElement(metadata))
            {
                return new BindingParameterContract()
                {
                    Convert = o => o != null ? (object)JsonSerializer.Serialize(o, metadata.Type, this.Options) : DBNull.Value,
                };
            }

            return null;
        }

        public IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata) => null;
        public IBindingValueContract GetValueContract(IBindingMetadata metadata)
        {
            if (!this.HasJsonAttribute(metadata) && !this.IsNativeJsonType(metadata))
                return null;

            return new BindingValueContract()
            {
                Convert = this.GetValueReaderProxy,
                Read = this.GetColumnReaderProxy,
            };
        }

        public IBindingHelperContract GetHelperContract(IBindingMetadata metadata) => this.HasJsonAttribute(metadata) ? this.helper : null;
    }
}
