using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.Nodes
{
    internal class ListNode : MemberNode
    {
        public int EnumeratorIndex { get; set; }
        public ItemNode Item { get; set; }
    }
}
