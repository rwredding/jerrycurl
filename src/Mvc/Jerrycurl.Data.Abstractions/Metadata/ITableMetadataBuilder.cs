using Jerrycurl.Relations.Metadata;
using System.Collections.Generic;

namespace Jerrycurl.Data.Metadata
{
    public interface ITableMetadataBuilder : IMetadataBuilder<ITableMetadata>, ICollection<ITableContractResolver>
    {
        ITableContractResolver DefaultResolver { get; set; }
    }
}
