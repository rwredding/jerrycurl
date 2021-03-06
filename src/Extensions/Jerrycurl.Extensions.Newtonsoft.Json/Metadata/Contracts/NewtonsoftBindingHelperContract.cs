﻿using Jerrycurl.Data.Metadata;
using Newtonsoft.Json;

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
