using System.Collections.Generic;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class ResultNode
    {
        public IList<ListNode> Lists { get; set; }
        public IList<ElementNode> Elements { get; set; }
        public IList<HelperNode> Helpers { get; set; }
    }
}
