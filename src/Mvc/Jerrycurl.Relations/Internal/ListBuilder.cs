using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Internal.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Relations.Internal
{
    internal class ListBuilder
    {
        private readonly RelationIdentity relation;
        private readonly MetadataIdentity source;

        public ListBuilder(RelationIdentity relation, MetadataIdentity source)
        {
            this.relation = relation ?? throw new ArgumentNullException(nameof(relation));
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public RelationNode Build()
        {
            this.VerifyHeading();

            ItemBuilder itemBuilder = new ItemBuilder(this.relation, this.source);

            ItemNode[] itemNodes = itemBuilder.Build().ToArray();
            ItemNode sourceNode = itemNodes.First(n => n.HasFlag(NodeFlags.Source));

            this.AssignLists(itemNodes);
            this.AssignIndices(sourceNode);

            MemberNode[] fieldNodes = this.EnumerateAllNodes(sourceNode).Where(n => n.FieldIndex != null).OrderBy(n => n.FieldIndex.Value).ToArray();

            return new RelationNode()
            {
                Items = itemNodes.OrderBy(n => n.ItemIndex).ToList(),
                Degree = fieldNodes.Select(n => n.FieldIndex.Value).DefaultIfEmpty().Max() + 1,
                VisibleDegree = this.relation.Heading.Count,
                Attributes = fieldNodes.Select(n => n.Metadata.Identity).ToArray(),
                Fields = fieldNodes,
                Identity = this.relation,
            };
        }

        private void VerifyHeading()
        {
            if (this.relation.Heading.Count != this.relation.Heading.Distinct().Count())
                throw RelationException.FromRelation(this.relation, $"Duplicate attribute found.");
        }

        private void AssignIndices(ItemNode sourceNode)
        {
            int fieldIndex = this.relation.Heading.Count;
            int itemIndex = 0;
            int enumeratorIndex = 0;

            foreach (ItemNode itemNode in this.EnumerateAllNodes(sourceNode).Where(n => n.HasFlag(NodeFlags.Item)).Cast<ItemNode>())
                itemNode.ItemIndex = itemIndex++;

            ListNode lastListNode = null;

            foreach (ListNode listNode in this.EnumerateAllNodes(sourceNode).Where(n => n.HasFlag(NodeFlags.List)).Cast<ListNode>())
            {
                listNode.EnumeratorIndex = enumeratorIndex++;

                if (!listNode.HasFlag(NodeFlags.Source) && listNode.FieldIndex == null)
                {
                    listNode.FieldIndex = fieldIndex++;
                    listNode.Flags |= NodeFlags.Field;
                }

                if (lastListNode != null && !lastListNode.Metadata.MemberOf.Equals(listNode.Metadata.MemberOf.Parent?.MemberOf))
                    listNode.Flags |= NodeFlags.Product;

                lastListNode = listNode;
            }
        }

        private void AssignLists(IEnumerable<ItemNode> itemNodes)
        {
            ItemNode source = itemNodes.First(n => n.HasFlag(NodeFlags.Source));

            foreach (ItemNode itemNode in itemNodes.Where(n => !n.HasFlag(NodeFlags.Source)))
            {
                ItemNode parentItem = source.Metadata.MemberOf.Equals(itemNode.Metadata.Parent.MemberOf) ? source : itemNodes.FirstOrDefault(n => n.Metadata == itemNode.Metadata.Parent.MemberOf);
                MemberNode listMember = parentItem.FindNode(itemNode.Metadata.Parent);
                MemberNode listParent = parentItem.FindNode(listMember.Metadata.Parent) ?? parentItem;

                ListNode listNode = new ListNode()
                {
                    Metadata = listMember.Metadata,
                    FieldIndex = listMember.FieldIndex,
                    Flags = NodeFlags.List,
                    Item = itemNode,
                };

                if (listMember.HasFlag(NodeFlags.Source))
                {
                    listNode.Flags |= NodeFlags.Source;
                    listNode.FieldIndex = null;
                }
                else if (listMember.HasFlag(NodeFlags.Field))
                    listNode.Flags |= NodeFlags.Field;

                listParent.Members.Remove(listMember);
                listParent.Members.Add(listNode);

                itemNode.List = listNode;
            }
        }

        private IEnumerable<MemberNode> EnumerateAllNodes(MemberNode node)
        {
            yield return node;

            if (node is ListNode listNode)
            {
                foreach (MemberNode node2 in this.EnumerateAllNodes(listNode.Item))
                    yield return node2;
            }

            foreach (MemberNode node2 in node.Members)
                foreach (MemberNode node3 in this.EnumerateAllNodes(node2))
                    yield return node3;
        }


    }
}
