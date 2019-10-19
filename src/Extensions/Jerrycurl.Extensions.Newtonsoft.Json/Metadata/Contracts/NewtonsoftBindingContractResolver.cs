using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Reflection;
using Newtonsoft.Json;

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
                Convert = o => o != null ? (object)JsonConvert.SerializeObject(o, this.Settings) : DBNull.Value,
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
