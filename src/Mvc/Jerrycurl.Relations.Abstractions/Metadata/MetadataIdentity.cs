using System;
using Jerrycurl.Diagnostics;
using Jerrycurl.Reflection;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Metadata
{
    public sealed class MetadataIdentity : IEquatable<MetadataIdentity>
    {
        public string Name { get; }
        public ISchema Schema { get; }
        public IMetadataNotation Notation => this.Schema.Notation;

        public MetadataIdentity(ISchema schema, string name)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public MetadataIdentity(ISchema schema)
            : this(schema, schema?.Notation?.Model())
        {

        }

        public TMetadata GetMetadata<TMetadata>()
            where TMetadata : IMetadata
            => this.Schema.GetMetadata<TMetadata>(this.Name);

        public MetadataIdentity Parent()
        {
            string parentName = this.Notation.Parent(this.Name);

            if (parentName != null)
                return new MetadataIdentity(this.Schema, parentName);

            return null;
        }

        public MetadataIdentity Child(string propertyName) => new MetadataIdentity(this.Schema, this.Notation.Combine(this.Name, propertyName));

        public bool Equals(MetadataIdentity other)
        {
            Equality eq = new Equality();

            eq.Add(this.Name, other.Name, this.Notation.Comparer);
            eq.Add(this.Schema, other.Schema);

            return eq.ToEquals();
        }

        public override bool Equals(object obj) => (obj is MetadataIdentity other && this.Equals(other));
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(this.Name, this.Notation.Comparer);
            hashCode.Add(this.Schema);

            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            if (this.Notation.Equals(this.Notation.Model(), this.Name))
                return $"{this.Schema.Model.GetSanitizedName()}:<model>";

            return $"{this.Schema.Model.GetSanitizedName()}:{this.Name}";
        }
    }
}
