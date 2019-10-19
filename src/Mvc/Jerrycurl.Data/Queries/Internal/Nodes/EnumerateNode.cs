using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class EnumerateNode
    {
        public IList<HelperNode> Helpers { get; set; }
        public IList<MetadataNode> Items { get; set; }
    }
}
