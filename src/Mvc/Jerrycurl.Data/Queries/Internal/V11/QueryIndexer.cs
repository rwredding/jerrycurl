using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryIndexer
    {
        private readonly ConcurrentDictionary<MetadataIdentity, int> slotMap = new ConcurrentDictionary<MetadataIdentity, int>();
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<MetadataIdentity, int>> listMap = new ConcurrentDictionary<int, ConcurrentDictionary<MetadataIdentity, int>>();
        private readonly ConcurrentDictionary<MetadataIdentity, int> aggregateMap = new ConcurrentDictionary<MetadataIdentity, int>();

        public int GetResultIndex() => 0;
        public int GetAggregateIndex(MetadataIdentity identity) => this.aggregateMap.GetOrAdd(identity, this.aggregateMap.Count);
        public int GetSlotIndex(MetadataIdentity parentIdentity, MetadataIdentity childIdentity) => this.slotMap.GetOrAdd(parentIdentity, this.slotMap.Count + 1);
        public int GetListIndex(MetadataIdentity parentIdentity, MetadataIdentity childIdentity)
        {
            return 0;
        }
    }
}
