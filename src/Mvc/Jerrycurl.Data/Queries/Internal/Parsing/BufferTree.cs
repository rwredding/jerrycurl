using System.Collections.Generic;
using Jerrycurl.Data.Queries.Internal.Binding;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal class BufferTree
    {
        public ISchema Schema { get; set; }
        public QueryType QueryType { get; set; }
        public IList<SlotWriter> Slots { get; } = new List<SlotWriter>();
        public IList<AggregateWriter> Aggregates { get; } = new List<AggregateWriter>();
        public IList<ListWriter> Lists { get; } = new List<ListWriter>();
        public IList<HelperWriter> Helpers { get; } = new List<HelperWriter>();

        public IList<AggregateName> AggregateNames { get; set; } = new List<AggregateName>();
    }
}
