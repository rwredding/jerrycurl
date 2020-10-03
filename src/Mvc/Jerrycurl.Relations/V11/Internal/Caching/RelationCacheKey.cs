using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11.Internal.Caching
{
    internal class RelationCacheKey : IEquatable<RelationCacheKey>
    {
        public RelationHeader Header { get; }
        public MetadataIdentity Source { get; }

        public RelationCacheKey(MetadataIdentity source, RelationHeader header)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Header = header ?? throw new ArgumentNullException(nameof(header));
        }

        public bool Equals(RelationCacheKey other) => Equality.Combine(this, other, m => m.Header, m => m.Source);
        public override int GetHashCode() => HashCode.Combine(this.Header, this.Source);
        public override bool Equals(object obj) => (obj is RelationCacheKey other && this.Equals(other));

    }
}
