using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public interface IReferenceMetadata : IMetadata, IEquatable<IReferenceMetadata>
    {
        Type Type { get; }
        IReadOnlyList<IReference> References { get; }
        IReadOnlyList<IReferenceKey> Keys { get; }
        ReferenceMetadataFlags Flags { get; }
        IRelationMetadata Relation { get; }
        IReadOnlyList<IReferenceMetadata> Properties { get; }
        IReferenceMetadata Item { get; }
        IReadOnlyList<Attribute> Annotations { get; }
    }
}
