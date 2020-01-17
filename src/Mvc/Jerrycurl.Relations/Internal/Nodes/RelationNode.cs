using Jerrycurl.Relations.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Relations.Internal.Nodes
{
    internal class RelationNode
    {
        public MetadataIdentity[] Attributes { get; set; }
        public IList<ItemNode> Items { get; set; }
        public int VisibleDegree { get; set; }
        public int Degree { get; set; }
        public IList<MemberNode> Fields { get; set; }
        public RelationIdentity Identity { get; set; }

        public IEnumerable<MemberNode> GetFields()
        {
            foreach (ItemNode itemNode in this.Items)
            {
                if (itemNode.List?.FieldIndex != null)
                    yield return itemNode.List;

                foreach (var x in itemNode.EnumerateNodes().Where(n => n.FieldIndex != null))
                    yield return x;
            }
        }
    }
}
