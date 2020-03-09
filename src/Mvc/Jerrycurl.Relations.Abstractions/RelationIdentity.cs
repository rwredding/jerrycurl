using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    public sealed class RelationIdentity : IEquatable<RelationIdentity>
    {
        public ISchema Schema { get; }
        public IReadOnlyList<MetadataIdentity> Heading { get; }

        public RelationIdentity(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Heading = Array.Empty<MetadataIdentity>();
        }

        public RelationIdentity(ISchema schema, IEnumerable<MetadataIdentity> heading)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Heading = heading?.ToArray() ?? throw new ArgumentNullException(nameof(heading));

            this.Validate();
        }

        private void Validate()
        {
            if (this.Heading.Any(m => m.Schema != this.Schema))
                throw new InvalidOperationException("Attributes must belong to same schema.");
        }

        public bool Equals(RelationIdentity other) => Equality.CombineAll(this.Heading, other?.Heading);
        public override bool Equals(object obj) => (obj is RelationIdentity other && this.Equals(other));
        public override int GetHashCode() => HashCode.CombineAll(this.Heading);

        public RelationIdentity Push(params string[] heading)
        {
            IEnumerable<MetadataIdentity> concatWith = heading.Select(n => new MetadataIdentity(this.Schema, n));

            return new RelationIdentity(this.Schema, this.Heading.Concat(concatWith));
        }

        public RelationIdentity Pop()
        {
            if (this.Heading.Count == 0)
                return null;

            return new RelationIdentity(this.Schema, this.Heading.Take(this.Heading.Count - 1));
        }
    }
}
