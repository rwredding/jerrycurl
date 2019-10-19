using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Metadata
{
    public interface IProjectionMetadata : IMetadata
    {
        Type Type { get; }
        ITableMetadata Table { get; }
        IRelationMetadata Relation { get; }
        IReferenceMetadata Reference { get; }
        IReadOnlyList<IProjectionMetadata> Properties { get; }
        ProjectionMetadataFlags Flags { get; }

        IProjectionMetadata Item { get; }
        IProjectionMetadata List { get; }
    }
}
