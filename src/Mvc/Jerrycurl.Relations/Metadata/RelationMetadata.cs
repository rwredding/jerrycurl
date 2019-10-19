using Jerrycurl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Relations.Metadata
{
    internal class RelationMetadata : IRelationMetadata
    {
        public MetadataIdentity Identity { get; }

        public IRelationMetadata Parent { get; set; }
        public IRelationMetadata MemberOf { get; set; }
        public RelationMetadata Item { get; set; }
        public Lazy<IReadOnlyList<RelationMetadata>> Properties { get; set; }
        public RelationMetadataFlags Flags { get; set; }

        public IReadOnlyList<Attribute> Annotations { get; set; } = Array.Empty<Attribute>();
        public MemberInfo Member { get; set; }
        public Type Type { get; set; }
        public MethodInfo ReadIndex { get; set; }
        public MethodInfo WriteIndex { get; set; }

        IEnumerable<IRelationMetadata> IRelationMetadata.Properties => this.Properties.Value;
        IRelationMetadata IRelationMetadata.Item => this.Item;

        public RelationMetadata(MetadataIdentity identity)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public bool Equals(IRelationMetadata other) => Equality.Combine(this.Identity, other?.Identity);
        public override int GetHashCode() => this.Identity.GetHashCode();
        public override bool Equals(object obj) => (obj is IRelationMetadata other && this.Equals(other));

        public override string ToString() => $"IRelationMetadata: {this.Identity}";
    }
}
