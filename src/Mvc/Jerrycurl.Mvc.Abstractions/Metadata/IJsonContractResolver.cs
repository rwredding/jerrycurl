using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc.Metadata
{
    public interface IJsonContractResolver
    {
        string GetPropertyName(IJsonMetadata metadata);
    }
}
