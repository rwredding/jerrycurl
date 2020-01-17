using Jerrycurl.Relations.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Relations.Internal.Nodes
{
    internal class MemberNode
    {
        public NodeFlags Flags { get; set; }
        public List<MemberNode> Members { get; set; } = new List<MemberNode>();
        public IRelationMetadata Metadata { get; set; }
        public int? FieldIndex { get; set; }

        public bool HasFlag(NodeFlags flags) => this.Flags.HasFlag(flags);

        public IEnumerable<MemberNode> EnumerateNodes()
        {
            IEnumerable<MemberNode> subNodes(MemberNode node)
            {
                yield return node;

                foreach (MemberNode memberNode in node.Members)
                    foreach (MemberNode node2 in subNodes(memberNode))
                        yield return node2;
            }

            return subNodes(this);
        }

        public MemberNode FindNode(IRelationMetadata metadata)
        {
            if (metadata == null)
                return null;

            return this.EnumerateNodes().FirstOrDefault(n => metadata.Equals(n.Metadata));
        }

        public override string ToString()
        {
            return this.Metadata.Identity.Name;
        }
    }
}
