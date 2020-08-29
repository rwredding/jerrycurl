using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Data.Queries.Internal.Extensions;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class BufferCache
    {
        private readonly Dictionary<BufferCacheKey, int> parentMap = new Dictionary<BufferCacheKey, int>();
        private readonly Dictionary<int, Dictionary<BufferCacheKey, int>> childMap = new Dictionary<int, Dictionary<BufferCacheKey, int>>();
        private readonly Dictionary<MetadataIdentity, int> aggregateMap = new Dictionary<MetadataIdentity, int>();
        private readonly object state = new object();

        public ISchema Schema { get; }

        public BufferCache(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public int GetResultIndex() => 0;
        public int GetAggregateIndex(MetadataIdentity metadata)
        {
            lock (this.state)
                return this.aggregateMap.GetOrAdd(metadata, this.aggregateMap.Count);
        }

        public int GetListIndex(MetadataIdentity metadata) => this.GetParentIndex(new BufferCacheKey(metadata));
        public int GetParentIndex(IReference reference)
        {
            IReference childReference = reference.Find(ReferenceFlags.Child);
            IReferenceMetadata metadata = childReference.List ?? childReference.Metadata;
            IReferenceKey parentKey = reference.HasFlag(ReferenceFlags.Self) ? childReference.Key : childReference.Other.Key;

            BufferCacheKey cacheKey = new BufferCacheKey(metadata.Identity, parentKey);

            return this.GetParentIndex(cacheKey);
        }

        public int GetChildIndex(IReference reference)
        {
            IReference childReference = reference.Find(ReferenceFlags.Child);
            IReferenceMetadata metadata = childReference.List ?? childReference.Metadata;
            IReferenceKey parentKey = reference.HasFlag(ReferenceFlags.Self) ? childReference.Key : childReference.Other.Key;

            int parentIndex = this.GetParentIndex(reference);

            lock (this.state)
            {
                Dictionary<BufferCacheKey, int> innerMap = this.childMap.GetOrAdd(parentIndex);
                BufferCacheKey joinKey = new BufferCacheKey(metadata.Identity, parentKey, childReference.Key);

                return innerMap.GetOrAdd(joinKey, innerMap.Count);
            }
        }

        private int GetParentIndex(BufferCacheKey key)
        {
            lock (this.state)
                return this.parentMap.GetOrAdd(key, this.parentMap.Count + 1);
        }
    }
}
