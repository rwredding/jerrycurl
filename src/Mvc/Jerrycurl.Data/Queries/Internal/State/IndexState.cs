using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Concurrent;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.State
{
    internal class IndexState
    {
        private readonly ConcurrentDictionary<MetadataIdentity, int> listMap = new ConcurrentDictionary<MetadataIdentity, int>();
        private readonly ConcurrentDictionary<IndexKey, int> parentMap = new ConcurrentDictionary<IndexKey, int>();
        private readonly ConcurrentDictionary<int, int> childSizes = new ConcurrentDictionary<int, int>();
        private readonly ConcurrentDictionary<IndexKey, int> childMap = new ConcurrentDictionary<IndexKey, int>();
        private readonly object state = new object();

        public int GetListIndex(MetadataIdentity identity) => this.listMap.GetOrAdd(identity, _ => this.listMap.Count);
        public int GetKeyIndex(IReference reference)
        {
            if (reference.HasFlag(ReferenceFlags.Child))
            {
                int parentIndex = this.GetParentIndex(reference.Other.Metadata.Identity, reference.Other.Key);

                return this.GetChildIndex(parentIndex, reference.Metadata.Identity, reference.Key);
            }
            else
                return this.GetParentIndex(reference.Metadata.Identity, reference.Key);
        }

        private int GetParentIndex(MetadataIdentity identity, IReferenceKey key)
        {
            IndexKey parentKey = new IndexKey(identity, key);

            if (this.parentMap.TryGetValue(parentKey, out int parentIndex))
                return parentIndex;

            lock (this.state)
            {
                return this.parentMap.GetOrAdd(parentKey, this.parentMap.Count);
            }
        }

        private int GetChildIndex(int parentIndex, MetadataIdentity identity, IReferenceKey key)
        {
            IndexKey childKey = new IndexKey(identity, key);

            if (this.childMap.TryGetValue(childKey, out int childIndex))
                return childIndex;

            lock (this.state)
            {
                if (!this.childMap.TryGetValue(childKey, out childIndex))
                {
                    childIndex = this.childSizes.TryGetValue(parentIndex);

                    this.childSizes[parentIndex] = childIndex + 1;
                    this.childMap.TryAdd(childKey, childIndex);
                }

                return childIndex;
            }
        }

        private class IndexKey : IEquatable<IndexKey>
        {
            public MetadataIdentity Identity { get; }
            public IReferenceKey Key { get; }

            public IndexKey(MetadataIdentity identity, IReferenceKey key)
            {
                this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
                this.Key = key ?? throw new ArgumentNullException(nameof(key));
            }

            public bool Equals(IndexKey other) => Equality.Combine(this, other, m => m.Identity, m => m.Key);
            public override bool Equals(object obj) => (obj is IndexKey other && this.Equals(other));
            public override int GetHashCode() => HashCode.Combine(this.Identity, this.Key);
        }
    }
}
