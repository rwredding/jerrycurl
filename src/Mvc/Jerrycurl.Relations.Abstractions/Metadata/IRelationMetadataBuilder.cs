using System.Collections.Generic;

namespace Jerrycurl.Relations.Metadata
{
    public interface IRelationMetadataBuilder : IMetadataBuilder<IRelationMetadata>, ICollection<IRelationContractResolver>
    {
        IRelationContractResolver DefaultResolver { get; set; }
    }
}
