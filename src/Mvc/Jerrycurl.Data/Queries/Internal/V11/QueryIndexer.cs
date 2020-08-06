using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Extensions;
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
        private readonly Dictionary<JoinEntry, int> parentMap = new Dictionary<JoinEntry, int>();
        private readonly Dictionary<int, Dictionary<JoinEntry, int>> childMap = new Dictionary<int, Dictionary<JoinEntry, int>>();
        private readonly Dictionary<MetadataIdentity, int> aggregateMap = new Dictionary<MetadataIdentity, int>();
        private readonly object state = new object();

        public int GetResultIndex() => 0;
        public int GetAggregateIndex(MetadataIdentity metadata)
        {
            lock (this.state)
                return this.aggregateMap.GetOrAdd(metadata, this.aggregateMap.Count);
        }

        public int GetListIndex(MetadataIdentity metadata) => this.GetParentIndex(new JoinEntry(metadata));
        public int GetParentIndex(IReference reference)
        {
            IReference parentReference = reference.Find(ReferenceFlags.Parent);
            JoinEntry joinKey = new JoinEntry(parentReference.Other.Metadata.Identity, parentReference.Key);

            return this.GetParentIndex(joinKey);
        }

        public int GetChildIndex(IReference reference)
        {
            IReference childReference = reference.Find(ReferenceFlags.Child);
            int parentIndex = this.GetParentIndex(reference);

            lock (this.state)
            {
                Dictionary<JoinEntry, int> innerMap = this.childMap.GetOrAdd(parentIndex);
                JoinEntry joinKey = new JoinEntry(childReference.Metadata.Identity, childReference.Other.Key, childReference.Key);

                return innerMap.GetOrAdd(joinKey, innerMap.Count);
            }
        }


        private int GetParentIndex(JoinEntry key)
        {
            lock (this.state)
                return this.parentMap.GetOrAdd(key, this.parentMap.Count + 1);
        }


        #region " Key "
        private class JoinEntry : IEquatable<JoinEntry>
        {
            public MetadataIdentity Metadata { get; }
            public IReadOnlyList<MetadataIdentity> ParentKey { get; set; }
            public IReadOnlyList<MetadataIdentity> ChildKey { get; set; }

            public JoinEntry(MetadataIdentity metadata)
            {
                this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            }

            public JoinEntry(MetadataIdentity metadata, IReferenceKey parentKey)
                : this(metadata)
            {
                this.ParentKey = parentKey.Properties.Select(m => m.Identity).ToList() ?? throw new ArgumentNullException(nameof(parentKey));
            }

            public JoinEntry(MetadataIdentity metadata, IReferenceKey parentKey, IReferenceKey childKey)
                : this(metadata, parentKey)
            {
                this.ChildKey = childKey?.Properties.Select(m => m.Identity).ToList() ?? throw new ArgumentNullException(nameof(childKey));
            }

            public bool Equals(JoinEntry other)
            {
                Equality eq = new Equality();

                eq.Add(this.Metadata, other?.Metadata);
                eq.AddRange(this.ParentKey, other?.ParentKey);
                eq.AddRange(this.ChildKey, other?.ChildKey);

                return eq.ToEquals();
            }
            public override bool Equals(object obj) => (obj is JoinEntry other && this.Equals(other));
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
        #endregion
    }
}
