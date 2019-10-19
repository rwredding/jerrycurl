using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Metadata
{
    public interface IRelationMetadataBuilder : IMetadataBuilder<IRelationMetadata>, ICollection<IRelationContractResolver>
    {
        IRelationContractResolver DefaultResolver { get; set; }
    }
}
