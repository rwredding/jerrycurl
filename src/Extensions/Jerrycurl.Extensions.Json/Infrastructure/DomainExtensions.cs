using System.Text.Json;
using Jerrycurl.Extensions.Json.Metadata.Contracts;

namespace Jerrycurl.Mvc
{
    public static class DomainExtensions
    {
        public static void UseJson(this DomainOptions options) => options.UseJson(null);

        public static void UseJson(this DomainOptions options, JsonSerializerOptions serializerOptions)
        {
            serializerOptions ??= new JsonSerializerOptions();

            options.Schemas.AddContract(new JsonBindingContractResolver(serializerOptions));
            options.Schemas.AddContract(new JsonContractResolver(serializerOptions));
        }
    }
}
