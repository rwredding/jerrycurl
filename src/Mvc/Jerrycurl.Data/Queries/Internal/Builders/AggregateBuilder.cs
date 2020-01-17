using System;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Nodes;
using Jerrycurl.Data.Queries.Internal.State;

namespace Jerrycurl.Data.Queries.Internal.Builders
{
    internal class AggregateBuilder
    {
        public TypeState State { get; }
        public ISchema Schema => this.State.Schema;

        public AggregateBuilder(TypeState state)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public AggregateNode Build()
        {
            IBindingMetadata metadata = this.Schema.GetMetadata<IBindingMetadata>();

            AggregateNode aggrNode = new AggregateNode()
            {
                Metadata = metadata,
                Index = this.State.Indexer.GetListIndex(metadata.Item.Identity),
            };

            if (this.State.Aggregate.Targets.Count > 0)
                aggrNode.Item = new MetadataNode(metadata.Item);

            foreach (MetadataIdentity identity in this.State.Aggregate.Targets)
            {
                IBindingMetadata targetMetadata = identity.GetMetadata<IBindingMetadata>();
                IBindingMetadata valueMetadata = targetMetadata.HasFlag(BindingMetadataFlags.Item) ? targetMetadata.Parent : targetMetadata;

                MetadataNode valueNode = new MetadataNode(valueMetadata)
                {
                    ListIndex = this.State.Indexer.GetListIndex(identity),
                };

                this.AddListNode(aggrNode, valueNode);
            }

            return aggrNode;
        }

        private void AddListNode(AggregateNode aggrNode, MetadataNode node)
        {
            MetadataNode parentNode = aggrNode.Item.FindNode(node.Metadata.Parent);
            MetadataNode thisNode = aggrNode.Item.FindNode(node.Metadata);

            if (thisNode == null && parentNode != null)
                parentNode.Properties.Add(node);
            else if (parentNode == null)
            {
                MetadataNode newParent = new MetadataNode(node.Metadata.Parent)
                {
                    Metadata = node.Metadata.Parent,
                };

                newParent.Properties.Add(node);

                this.AddListNode(aggrNode, newParent);
            }
        }
    }
}
