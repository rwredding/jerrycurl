using System;
using System.Collections.Generic;
using System.Reflection;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingMetadata : IMetadata, IEquatable<IBindingMetadata>
    {
        Type Type { get; }
        MemberInfo Member { get; }
        BindingMetadataFlags Flags { get; }
        IBindingMetadata Parent { get; }
        IBindingMetadata Item { get; }
        IBindingMetadata MemberOf { get; }
        IReadOnlyList<IBindingMetadata> Properties { get; }
        IRelationMetadata Relation { get; }
        IReadOnlyList<Attribute> Annotations { get; }

        IBindingParameterContract Parameter { get; }
        IBindingCompositionContract Composition { get; }
        IBindingValueContract Value { get; }
        IBindingHelperContract Helper { get; }
    }
}
