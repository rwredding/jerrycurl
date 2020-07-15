using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal class AggregateNode : Node
    {
        public IList<ListNode> Lists { get; set; }
        public IList<ScalarNode> Scalars { get; set; }
    }
}
