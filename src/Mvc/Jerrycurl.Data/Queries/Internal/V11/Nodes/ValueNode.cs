using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal class ValueNode : Node
    {
        public IList<ValueNode> Properties { get; set; }
        public JoinNode Joins { get; set; }
        public KeyNode PrimaryKey { get; set; }
    }
}
