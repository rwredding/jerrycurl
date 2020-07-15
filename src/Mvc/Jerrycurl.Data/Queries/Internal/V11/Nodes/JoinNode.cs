using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal class JoinNode
    {
        public KeyNode Key { get; set; }
        public IList<ListNode> Lists { get; set; }
        public int ListIndex { get; set; }
    }
}
