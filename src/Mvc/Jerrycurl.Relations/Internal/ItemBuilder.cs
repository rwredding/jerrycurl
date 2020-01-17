using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Internal.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Relations.Internal
{
    internal class ItemBuilder
    {
        private readonly RelationIdentity relation;
        private readonly IRelationMetadata sourceMetadata;
        private readonly IRelationMetadata[] headingMetadata;

        public ItemBuilder(RelationIdentity relation, MetadataIdentity source)
        {
            this.relation = relation ?? throw new ArgumentNullException(nameof(relation));
            this.sourceMetadata = source.GetMetadata<IRelationMetadata>();
            this.headingMetadata = relation.Heading.Select(m => m.GetMetadata<IRelationMetadata>()).ToArray();

            this.Validate();
        }

        public IEnumerable<ItemNode> Build()
        {
            ItemNode[] itemNodes = this.GetItemNodes().ToArray();

            Array.ForEach(itemNodes, this.AssignIndices);

            return itemNodes;
        }

        private void Validate()
        {
            for (int i = 0; i < this.headingMetadata.Length; i++)
            {
                if (this.headingMetadata[i] == null)
                    throw RelationException.FromRelation(this.relation, $"Metadata for attribute name '{this.relation.Heading[i]}' not found at index {i}.");
            }
        }

        private void AssignIndices(ItemNode itemNode)
        {
            foreach (MemberNode node in itemNode.EnumerateNodes())
            {
                int fieldIndex = Array.IndexOf(this.headingMetadata, node.Metadata);

                if (fieldIndex > -1)
                {
                    node.FieldIndex = fieldIndex;
                    node.Flags |= NodeFlags.Field;
                }
            }
        }

        private IEnumerable<ItemNode> GetItemNodes()
        {
            IRelationMetadata[] attributes = this.headingMetadata.SelectMany(this.Path).Distinct().ToArray();

            foreach (var g in attributes.GroupBy(a => a.MemberOf))
            {
                ItemNode itemNode = new ItemNode()
                {
                    Flags = NodeFlags.Item,
                };

                if (g.Contains(this.sourceMetadata))
                {
                    itemNode.Metadata = this.sourceMetadata;
                    itemNode.Flags |= NodeFlags.Source;
                }
                else
                    itemNode.Metadata = g.Key;

                foreach (IRelationMetadata metadata in g.Except(new[] { itemNode.Metadata }))
                {
                    MemberNode memberNode = this.CreateMemberNode(metadata);

                    this.AddMemberNode(itemNode, memberNode);
                }

                yield return itemNode;
            }
        }

        private MemberNode CreateMemberNode(IRelationMetadata metadata)
        {
            return new MemberNode()
            {
                Metadata = metadata,
            };
        }

        private void AddMemberNode(ItemNode itemNode, MemberNode node)
        {
            MemberNode parentNode = itemNode.FindNode(node.Metadata?.Parent);
            MemberNode thisNode = itemNode.FindNode(node.Metadata);

            if (thisNode == null && parentNode != null)
                parentNode.Members.Add(node);
            else if (node != itemNode && parentNode == null)
            {
                MemberNode newParent = this.CreateMemberNode(node.Metadata.Parent);

                newParent.Members.Add(node);

                this.AddMemberNode(itemNode, newParent);
            }
        }

        private IEnumerable<IRelationMetadata> Path(IRelationMetadata metadata)
        {
            IRelationMetadata origin = metadata;

            while (!this.sourceMetadata.Equals(metadata) && metadata != null)
            {
                yield return metadata;

                metadata = metadata.Parent;
            }

            if (metadata == null)
                throw RelationException.FromRelation(this.relation, $"No relationship found between '{this.sourceMetadata.Identity.Name}' and '{origin.Identity.Name}'.");

            yield return metadata;
        }
    }
}
