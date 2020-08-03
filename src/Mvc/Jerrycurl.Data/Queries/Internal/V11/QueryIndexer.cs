using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryIndexer
    {
        private readonly Dictionary<JoinKey, int> slotMap = new Dictionary<JoinKey, int>();
        private readonly Dictionary<int, Dictionary<JoinKey, int>> listMap = new Dictionary<int, Dictionary<JoinKey, int>>();
        private readonly Dictionary<MetadataIdentity, int> aggregateMap = new Dictionary<MetadataIdentity, int>();
        private readonly object state = new object();

        public int GetResultIndex() => 0;
        public int GetAggregateIndex(MetadataIdentity metadata)
        {
            lock (this.state)
                return this.aggregateMap.GetOrAdd(metadata, this.aggregateMap.Count);
        }

        public int GetSlotIndex(MetadataIdentity metadata)
            => this.GetSlotIndex(new JoinKey(metadata));

        public int GetSlotIndex(MetadataIdentity metadata, IReferenceKey parentKey)
            => this.GetSlotIndex(new JoinKey(metadata, parentKey));

        public int GetListIndex(MetadataIdentity metadata, IReferenceKey parentKey, IReferenceKey childKey)
        {
            lock (this.state)
            {
                int slotIndex = this.GetSlotIndex(metadata, parentKey);
                Dictionary<JoinKey, int> innerMap = this.listMap.GetOrAdd(slotIndex);

                return innerMap.GetOrAdd(new JoinKey(metadata, parentKey, childKey), innerMap.Count);
            }
        }

        private int GetSlotIndex(JoinKey key)
        {
            lock (this.state)
                return this.slotMap.GetOrAdd(key, this.slotMap.Count + 1);
        }


        #region " Key "
        private class JoinKey : IEquatable<JoinKey>
        {
            public MetadataIdentity Metadata { get; }
            public IReadOnlyList<MetadataIdentity> ParentKey { get; set; }
            public IReadOnlyList<MetadataIdentity> ChildKey { get; set; }

            public JoinKey(MetadataIdentity metadata)
            {
                this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            }

            public JoinKey(MetadataIdentity metadata, IReferenceKey parentKey)
                : this(metadata)
            {
                this.ParentKey = parentKey.Properties.Select(m => m.Identity).ToList() ?? throw new ArgumentNullException(nameof(parentKey));
            }

            public JoinKey(MetadataIdentity metadata, IReferenceKey parentKey, IReferenceKey childKey)
                : this(metadata, parentKey)
            {
                this.ChildKey = childKey?.Properties.Select(m => m.Identity).ToList() ?? throw new ArgumentNullException(nameof(childKey));
            }

            public bool Equals(JoinKey other)
            {
                Equality eq = new Equality();

                eq.Add(this.Metadata, other?.Metadata);
                eq.AddCollection(this.ParentKey, other?.ParentKey);
                eq.AddCollection(this.ChildKey, other?.ChildKey);

                return eq.ToEquals();
            }
            public override bool Equals(object obj) => (obj is JoinKey other && this.Equals(other));
            public override int GetHashCode()
            {
                HashCode hashCode = new HashCode();

                hashCode.Add(this.Metadata);
                hashCode.Add(this.ParentKey);
                hashCode.Add(this.ChildKey);

                return hashCode.ToHashCode();
            }
        }
        #endregion
    }
}
