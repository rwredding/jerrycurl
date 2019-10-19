using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.Nodes
{
    internal class ItemNode : MemberNode
    {
        public ListNode List { get; set; }
        public int ItemIndex { get; set; }
    }
}