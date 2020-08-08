using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class AggregateBuffer
    {
        public ISchema Schema { get; }
        public List<AggregateName> Names { get; } = new List<AggregateName>();
        public ElasticArray Values { get; } = new ElasticArray();

        public AggregateBuffer(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public QueryCacheKey<AggregateName> ToCacheKey() => new QueryCacheKey<AggregateName>(this.Schema, this.Names);
    }
}
