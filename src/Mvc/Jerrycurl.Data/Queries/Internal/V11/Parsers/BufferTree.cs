using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class BufferTree
    {
        public ISchema Schema { get; set; }
        public IList<SlotWriter> Slots { get; set; }
        public IList<AggregateWriter> Aggregates { get; set; }
        public IList<ListWriter> Lists { get; set; }
        public IList<HelperWriter> Helpers { get; set; }
    }
}
