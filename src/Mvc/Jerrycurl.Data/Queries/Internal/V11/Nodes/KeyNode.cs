using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal class KeyNode
    {
        public IList<Type> Type { get; set; }
        public IList<ScalarNode> Values { get; set; }
    }
}
