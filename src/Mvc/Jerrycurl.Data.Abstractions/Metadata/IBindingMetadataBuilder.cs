using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingMetadataBuilder : IMetadataBuilder<IBindingMetadata>, ICollection<IBindingContractResolver>
    {
        IBindingContractResolver DefaultResolver { get; set; }
    }
}
