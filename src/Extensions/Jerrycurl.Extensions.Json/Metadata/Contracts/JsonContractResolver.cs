using Jerrycurl.Mvc.Metadata;
using System;
using System.Reflection;
using System.Text.Json;

namespace Jerrycurl.Extensions.Json.Metadata.Contracts
{
    internal class JsonContractResolver : IJsonContractResolver
    {
        public JsonSerializerOptions Options { get; }

        public JsonContractResolver(JsonSerializerOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string GetPropertyName(IJsonMetadata metadata)
        {
            PropertyInfo propertyInfo = metadata.Relation.Member as PropertyInfo;

            if (propertyInfo != null)
                return this.Options?.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;

            return null;
        }
    }
}
