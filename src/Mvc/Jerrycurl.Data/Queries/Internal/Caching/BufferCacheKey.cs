using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class BufferCacheKey : IEquatable<BufferCacheKey>
    {
        public MetadataIdentity Metadata { get; }
        public IReadOnlyList<MetadataIdentity> ParentKey { get; set; }
        public IReadOnlyList<MetadataIdentity> ChildKey { get; set; }

        public BufferCacheKey(MetadataIdentity metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public BufferCacheKey(MetadataIdentity metadata, IReferenceKey parentKey)
            : this(metadata)
        {
            this.ParentKey = parentKey.Properties.Select(m => m.Identity).ToList() ?? throw new ArgumentNullException(nameof(parentKey));
        }

        public BufferCacheKey(MetadataIdentity metadata, IReferenceKey parentKey, IReferenceKey childKey)
            : this(metadata, parentKey)
        {
            this.ChildKey = childKey?.Properties.Select(m => m.Identity).ToList() ?? throw new ArgumentNullException(nameof(childKey));
        }

        public bool Equals(BufferCacheKey other)
        {
            Equality eq = new Equality();

            eq.Add(this.Metadata, other?.Metadata);
            eq.AddRange(this.ParentKey, other?.ParentKey);
            eq.AddRange(this.ChildKey, other?.ChildKey);

            return eq.ToEquals();
        }
        public override bool Equals(object obj) => (obj is BufferCacheKey other && this.Equals(other));
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(this.Metadata);

            if (this.ParentKey != null)
                hashCode.AddRange(this.ParentKey);

            if (this.ChildKey != null)
                hashCode.AddRange(this.ChildKey);

            return hashCode.ToHashCode();
        }
    }
}
