using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class AggregateTree
    {
        public ISchema Schema { get; set; }
        public NodeBinder Aggregate { get; set; }
    }
}
