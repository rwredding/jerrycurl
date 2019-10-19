using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    internal class ReferenceMetadata : IReferenceMetadata
    {
        public MetadataIdentity Identity { get; }
        public IRelationMetadata Relation { get; }
        public Type Type { get; }

        public ReferenceMetadataFlags Flags { get; set; }
        public Lazy<IReadOnlyList<ReferenceKey>> Keys { get; set; }
        public Lazy<IReadOnlyList<Reference>> References { get; set; }
        public Lazy<IReadOnlyList<ReferenceMetadata>> Properties { get; set; }
        public ReferenceMetadata Item { get; set; }
        public IReadOnlyList<Attribute> Annotations => this.Relation.Annotations;

        public Lazy<IReadOnlyList<Reference>> ParentReferences { get; set; }
        public Lazy<IReadOnlyList<Reference>> ChildReferences { get; set; }

        public ReferenceMetadata Parent { get; set; }

        IReadOnlyList<IReferenceMetadata> IReferenceMetadata.Properties => this.Properties?.Value;
        IReferenceMetadata IReferenceMetadata.Item => this.Item;
        IReadOnlyList<IReference> IReferenceMetadata.References => this.ParentReferences.Value.Concat(this.ChildReferences.Value).ToList();
        IReadOnlyList<IReferenceKey> IReferenceMetadata.Keys => this.Keys?.Value;

        public ReferenceMetadata(IRelationMetadata relation)
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));

            this.Identity = relation.Identity;
            this.Type = relation.Type;
            this.Relation = relation;
        }

        public bool Equals(IReferenceMetadata other) => Equality.Combine(this.Identity, other?.Identity);
        public override int GetHashCode() => this.Identity.GetHashCode();
        public override bool Equals(object obj) => (obj is IReferenceMetadata other && this.Equals(other));

        public override string ToString() => $"IReferenceMetadata: {this.Identity}";

    }
}