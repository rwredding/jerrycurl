using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jerrycurl.Relations.Metadata
{
    public interface IRelationMetadata : IMetadata, IEquatable<IRelationMetadata>
    {
        IEnumerable<IRelationMetadata> Properties { get; }
        IRelationMetadata Parent { get; }
        IRelationMetadata MemberOf { get; }
        IRelationMetadata Item { get; }
        RelationMetadataFlags Flags { get; }
        IReadOnlyList<Attribute> Annotations { get; }

        MethodInfo WriteIndex { get; }
        MethodInfo ReadIndex { get; }
        MemberInfo Member { get; }
        Type Type { get; }
    }
}
