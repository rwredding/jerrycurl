using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    public sealed class RelationIdentity2 : IEquatable<RelationIdentity2>
    {
        public ISchema Schema { get; }
        public IReadOnlyList<MetadataIdentity> Heading { get; }
        //public IReadOnlyList<IRelationMetadata> Metadata { get; }

        public RelationIdentity2(ISchema schema, IEnumerable<MetadataIdentity> heading)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Heading = heading?.ToArray() ?? throw new ArgumentNullException(nameof(heading));
            //this.Metadata = heading.Select(m => m.GetMetadata<IRelationMetadata>()).ToList();

            this.Validate();
        }

        private void Validate()
        {
            if (this.Heading.Any(m => m.Schema != this.Schema))
                throw new InvalidOperationException("Attributes must belong to same schema.");
        }

        public bool Equals(RelationIdentity2 other) => Equality.CombineAll(this.Heading, other?.Heading);
        public override bool Equals(object obj) => (obj is RelationIdentity2 other && this.Equals(other));
        public override int GetHashCode() => HashCode.CombineAll(this.Heading);

        public RelationIdentity2 Push(params string[] heading)
        {
            IEnumerable<MetadataIdentity> concatWith = heading.Select(n => new MetadataIdentity(this.Schema, n));

            return new RelationIdentity2(this.Schema, this.Heading.Concat(concatWith));
        }

        public RelationIdentity2 Pop()
        {
            if (this.Heading.Count == 0)
                return null;

            return new RelationIdentity2(this.Schema, this.Heading.Take(this.Heading.Count - 1));
        }
    }
}
