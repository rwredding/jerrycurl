using Jerrycurl.Relations.Metadata;
using System.Collections.Generic;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingMetadataBuilder : IMetadataBuilder<IBindingMetadata>, ICollection<IBindingContractResolver>
    {
        IBindingContractResolver DefaultResolver { get; set; }
    }
}
