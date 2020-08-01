using Jerrycurl.Data.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jerrycurl.Extensions.Newtonsoft.Json.Metadata.Contracts
{
    internal class NewtonsoftBindingHelperContract : BindingHelperContract<JsonSerializerSettings>
    {
        public NewtonsoftBindingHelperContract(JsonSerializerSettings settings)
            : base(settings)
        {
            
        }
    }
}
