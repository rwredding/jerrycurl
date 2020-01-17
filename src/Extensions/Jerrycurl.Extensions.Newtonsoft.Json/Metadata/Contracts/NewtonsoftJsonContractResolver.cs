using Jerrycurl.Mvc.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Jerrycurl.Extensions.Newtonsoft.Json.Metadata.Contracts
{
    internal class NewtonsoftJsonContractResolver : IJsonContractResolver
    {
        public JsonSerializerSettings Settings { get; }

        public NewtonsoftJsonContractResolver(JsonSerializerSettings settings)
        {
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string GetPropertyName(IJsonMetadata metadata)
        {
            PropertyInfo propertyInfo = metadata.Relation.Member as PropertyInfo;

            if (propertyInfo != null)
            {
                string propertyName = propertyInfo.Name;

                if (this.Settings.ContractResolver is DefaultContractResolver dcr)
                    propertyName = dcr.NamingStrategy?.GetPropertyName(propertyName, false) ?? propertyName;

                JsonPropertyAttribute propAttr = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();

                if (propAttr?.PropertyName != null)
                    propertyName = propAttr.PropertyName;

                return propertyName;
            }

            return null;
        }
    }
}
