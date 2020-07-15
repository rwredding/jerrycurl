using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal class ScalarNode : ValueNode
    {
        public ColumnIdentity Column { get; set; }
        public int? AggregateIndex { get; set; }
    }
}
