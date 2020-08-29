using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Metadata
{
    internal class Reference : IReference
    {
        public Reference Other { get; set; }
        public ReferenceFlags Flags { get; set; }
        public ReferenceKey Key { get; set; }
        public ReferenceMetadata Metadata { get; set; }
        public ReferenceMetadata List { get; set; }

        IReferenceMetadata IReference.Metadata => this.Metadata;
        IReferenceMetadata IReference.List => this.List;
        IReference IReference.Other => this.Other;
        IReferenceKey IReference.Key => this.Key;

        public bool Equals(IReference other) => Equality.Combine(this, other, m => m.Key, m => m.Metadata.Identity);
        public override bool Equals(object obj) => (obj is IReference other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Key, this.Metadata.Identity);

        public override string ToString() => this.Metadata.Identity + " -> " + this.Other.Metadata.Identity;
    }
}
