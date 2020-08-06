using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.V11.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class AggregateBuffer
    {
        private readonly List<AggregateValue> xs = new List<AggregateValue>();
        public ISchema Schema { get; set; }

        public AggregateBuffer(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public void Add(IEnumerable<AggregateValue> xs) => this.xs.AddRange(xs);

        public ExpandingArray Data { get; set; }

        public QueryCacheKey<AggregateValue> ToCacheKey() => new QueryCacheKey<AggregateValue>(this.Schema, this.xs);
    }
}
