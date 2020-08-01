using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal interface IQueryBuffer
    {
        public AggregateIdentity Aggregator { get; }
        public ElasticArray Slots { get; }
        public ElasticArray Aggregates { get; }
    }
}
