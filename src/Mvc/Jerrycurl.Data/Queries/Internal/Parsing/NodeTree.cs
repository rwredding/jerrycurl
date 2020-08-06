using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal class NodeTree
    {
        public IList<Node> Nodes { get; } = new List<Node>();
        public IList<Node> Items { get; } = new List<Node>();

        public Node FindNode(IBindingMetadata metadata) => this.FindNode(metadata?.Identity);
        public Node FindNode(MetadataIdentity identity) => this.Nodes.FirstOrDefault(n => n.Identity.Equals(identity));
    }
}
