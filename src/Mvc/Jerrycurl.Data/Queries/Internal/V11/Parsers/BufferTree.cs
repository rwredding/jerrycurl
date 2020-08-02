using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Data.Queries.Internal.V11.Writers;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class BufferTree
    {
        public ISchema Schema { get; set; }
        public IList<SlotWriter> Slots { get; } = new List<SlotWriter>();
        public IList<AggregateWriter> Aggregates { get; } = new List<AggregateWriter>();
        public IList<ListWriter> Lists { get; } = new List<ListWriter>();
        public IList<HelperWriter> Helpers { get; } = new List<HelperWriter>();
    }
}
