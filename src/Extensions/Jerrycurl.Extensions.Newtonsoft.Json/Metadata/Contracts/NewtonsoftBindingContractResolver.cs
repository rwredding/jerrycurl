using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jerrycurl.Extensions.Newtonsoft.Json.Metadata.Contracts
{
    public class NewtonsoftBindingContractResolver : IBindingContractResolver
    {
        public int Priority => 1;

        public JsonSerializerSettings Settings { get; }

        private readonly NewtonsoftBindingHelperContract helper;

        public NewtonsoftBindingContractResolver(JsonSerializerSettings settings)
        {
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.helper = new NewtonsoftBindingHelperContract(settings);
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

            if (valueInfo.TargetType == typeof(JToken))
                targetValue = this.GetParseJTokenExpression(valueInfo.Metadata, value);
            else
                targetValue = this.GetDeserializeExpression(valueInfo.Metadata, value, valueInfo.Helper);

            if (nullCheck != null)
                return Expression.Condition(nullCheck, Expression.Default(targetValue.Type), targetValue);

            return targetValue;
        }

        private Expression GetParseJTokenExpression(IBindingMetadata metadata, Expression value)
        {
            MethodInfo parseMethod = typeof(JToken).GetMethod(nameof(JToken.Parse), new[] { typeof(string) });

            return Expression.Call(parseMethod, value);
        }


        private Expression GetDeserializeExpression(IBindingMetadata metadata, Expression value, Expression helper)
        {
            IEnumerable<MethodInfo> methods = typeof(JsonConvert).GetMethods().Where(m => m.Name == "DeserializeObject" && m.ContainsGenericParameters);

            if (helper != null)
            {
                MethodInfo deserializeObject = methods.Select(mi => mi.MakeGenericMethod(metadata.Type)).FirstOrDefault(mi => mi.HasParameters(typeof(string), helper.Type));

                return Expression.Call(deserializeObject, value, helper);
            }
            else
            {
                MethodInfo deserializeObject = methods.Select(mi => mi.MakeGenericMethod(metadata.Type)).FirstOrDefault(mi => mi.HasParameters(typeof(string)));

                return Expression.Call(deserializeObject, value);
            }
        }

        private bool HasJsonAttribute(IBindingMetadata metadata) => metadata.Relation.Annotations.OfType<JsonAttribute>().Any();
        private bool IsNativeJToken(IBindingMetadata metadata) => (metadata.Type == typeof(JToken));

        public IBindingParameterContract GetParameterContract(IBindingMetadata metadata)
        {
            if (this.IsNativeJToken(metadata))
            {
                return new BindingParameterContract()
                {
                    Convert = o => (object)((JObject)o)?.ToString() ?? DBNull.Value,
                };
            }
            else if (this.HasJsonAttribute(metadata))
            {
                return new BindingParameterContract()
                {
                    Convert = o => o != null ? (object)JsonConvert.SerializeObject(o, this.Settings) : DBNull.Value,
                };
            }

            return null;
        }
        public IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata) => null;
        public IBindingValueContract GetValueContract(IBindingMetadata metadata)
        {
            if (!this.HasJsonAttribute(metadata) && !this.IsNativeJToken(metadata))
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
