using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    [DebuggerDisplay("{Schema.ToString(),nq}({Name})")]
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

        public override string ToString() => this.Schema.Notation.Model().Equals(this.Metadata.Name) ? "<model>" : this.Name;

        public FieldIdentity Pop()
        {
            string newName = this.Schema.Notation.Parent(this.Name);

            if (newName != null)
                return new FieldIdentity(this.Metadata.Pop(), newName);

            return null;
        }

        public FieldIdentity Push(string propertyName)
        {
            string newName = this.Schema.Notation.Combine(this.Name, propertyName);

            return new FieldIdentity(this.Metadata.Push(propertyName), newName);
        }

        public FieldIdentity Push(string itemName, int index)
        {
            string newName = this.Schema.Notation.Combine(this.Name, this.Schema.Notation.Index(itemName, index));

            return new FieldIdentity(this.Metadata.Push(itemName), newName);
        }
    }
}
