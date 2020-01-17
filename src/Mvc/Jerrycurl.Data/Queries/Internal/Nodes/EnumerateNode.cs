using System.Collections.Generic;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class EnumerateNode
    {
        public IList<HelperNode> Helpers { get; set; }
        public IList<MetadataNode> Items { get; set; }
    }
}
