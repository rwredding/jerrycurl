using System.Text.Json;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Extensions.Json.Metadata.Contracts
{
    internal class JsonBindingHelperContract : BindingHelperContract<JsonSerializerOptions>
    {
        public JsonBindingHelperContract(JsonSerializerOptions options)
            : base(options)
        {

        }
    }
}
