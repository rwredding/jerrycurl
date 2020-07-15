using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal class ListNode : Node
    {
        public KeyNode JoinKey { get; set; }
        public ItemNode Item { get; set; }
        public int ListIndex { get; set; }
    }
}
