using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class AggregateBuffer
    {
        private readonly List<AggregateName> names = new List<AggregateName>();

        public ISchema Schema { get; }
        public List<AggregateName> Names { get; } = new List<AggregateName>();
        public ElasticArray Values { get; } = new ElasticArray();

        public AggregateBuffer(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public void Add(IEnumerable<AggregateName> names) => this.names.AddRange(names);

        public QueryCacheKey<AggregateName> ToCacheKey() => new QueryCacheKey<AggregateName>(this.Schema, this.names);
    }
}
