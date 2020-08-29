using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Internal.V11.Caching
{
    internal class RelationCacheKey : IEquatable<RelationCacheKey>
    {
        public RelationIdentity2 Relation { get; }
        public MetadataIdentity Source { get; }

        public RelationCacheKey(MetadataIdentity source, RelationIdentity2 relation)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }

        public bool Equals(RelationCacheKey other) => Equality.Combine(this, other, m => m.Relation, m => m.Source);
        public override int GetHashCode() => HashCode.Combine(this.Relation, this.Source);
        public override bool Equals(object obj) => (obj is RelationCacheKey other && this.Equals(other));

    }
}
