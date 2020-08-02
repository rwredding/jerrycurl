using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class NodeParser
    {
        public ISchema Schema { get; }

        public NodeParser(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public NodeTree Parse(IEnumerable<MetadataIdentity> metadata)
        {
            NodeTree tree = new NodeTree();

            foreach (MetadataIdentity identity in metadata)
                this.AddNode(tree, identity);

            return tree;
        }

        private void AddNode(NodeTree tree, MetadataIdentity identity)
        {
            IBindingMetadata metadata = identity.GetMetadata<IBindingMetadata>() ?? this.FindDynamicMetadata(identity);

            if (this.IsValidMetadata(metadata))
            {
                if (metadata.HasFlag(BindingMetadataFlags.Dynamic))
                    this.AddDynamicNode(tree, identity, metadata);
                else
                    this.AddStaticNode(tree, metadata);
            }
        }

        private Node AddDynamicNode(NodeTree tree, MetadataIdentity identity, IBindingMetadata metadata)
        {
            this.AddStaticNode(tree, metadata);

            Node thisNode = tree.FindNode(identity);
            MetadataIdentity parentIdentity = identity.Pop();

            if (thisNode != null)
                return thisNode;
            else if (parentIdentity != null)
            {
                Node parentNode = tree.FindNode(parentIdentity) ?? this.AddDynamicNode(tree, parentIdentity, metadata);

                if (parentNode != null)
                {
                    thisNode = new Node(identity, metadata)
                    {
                        Flags = NodeFlags.Dynamic,
                    };

                    parentNode.Properties.Add(thisNode);
                    tree.Nodes.Add(thisNode);
                }
            }

            return thisNode;
        }

        private Node AddStaticNode(NodeTree tree, IBindingMetadata metadata)
        {
            Node thisNode = tree.FindNode(metadata);

            if (thisNode != null)
                return thisNode;
            else if (metadata.HasFlag(BindingMetadataFlags.Item))
            {
                thisNode = new Node(metadata)
                {
                    Flags = NodeFlags.Item,
                };

                if (metadata.HasFlag(BindingMetadataFlags.Dynamic))
                    thisNode.Flags |= NodeFlags.Dynamic;

                if (metadata.Parent.HasFlag(BindingMetadataFlags.Model))
                    thisNode.Flags |= NodeFlags.Result;

                tree.Nodes.Add(thisNode);
                tree.Items.Add(thisNode);
            }
            else
            {
                Node parentNode = tree.FindNode(metadata.Parent) ?? this.AddStaticNode(tree, metadata.Parent);

                if (parentNode != null)
                {
                    thisNode = new Node(metadata)
                    {
                        Flags = metadata.HasFlag(BindingMetadataFlags.Dynamic) ? NodeFlags.Dynamic : NodeFlags.None,
                    };

                    parentNode.Properties.Add(thisNode);
                    tree.Nodes.Add(thisNode);
                }
            }

            return thisNode;
        }

        private bool IsValidMetadata(IBindingMetadata metadata) => (metadata != null && !metadata.MemberOf.HasFlag(BindingMetadataFlags.Model));

        private IBindingMetadata FindDynamicMetadata(MetadataIdentity identity)
        {
            IBindingMetadata metadata = identity.GetMetadata<IBindingMetadata>();

            while (metadata == null && (identity = identity.Pop()) != null)
                metadata = identity.GetMetadata<IBindingMetadata>();

            if (metadata != null && metadata.HasFlag(BindingMetadataFlags.Dynamic))
                return metadata;

            return null;
        }
    }
}
