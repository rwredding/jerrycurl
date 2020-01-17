using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    public sealed class FieldIdentity : IEquatable<FieldIdentity>
    {
        public string Name { get; }
        public ISchema Schema => this.Metadata.Schema;
        public MetadataIdentity Metadata { get; }

        public FieldIdentity(MetadataIdentity metadata, string name)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public TMetadata GetMetadata<TMetadata>()
            where TMetadata : IMetadata
            => this.Metadata.GetMetadata<TMetadata>();

        public bool Equals(FieldIdentity other) => Equality.Combine(this, other, m => m.Metadata, m => m.Name);
        public override bool Equals(object obj) => (obj is FieldIdentity other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Metadata, this.Name);
    }
}
