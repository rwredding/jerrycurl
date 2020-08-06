using System;
using System.Collections.Generic;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal static class NodeParser
    {
        public static NodeTree Parse(ISchema schema, IEnumerable<IValueName> valueNames)
        {
            NodeTree tree = new NodeTree();

            foreach (IValueName name in valueNames)
                AddNode(tree, new MetadataIdentity(schema, name.Name));

            return tree;
        }

        private static void AddNode(NodeTree tree, MetadataIdentity identity)
        {
            IBindingMetadata metadata = identity.GetMetadata<IBindingMetadata>() ?? FindDynamicMetadata(identity);

            if (IsValidMetadata(metadata))
            {
                if (metadata.HasFlag(BindingMetadataFlags.Dynamic))
                    AddDynamicNode(tree, identity, metadata);
                else
                    AddStaticNode(tree, metadata);
            }
        }

        private static Node AddDynamicNode(NodeTree tree, MetadataIdentity identity, IBindingMetadata metadata)
        {
            AddStaticNode(tree, metadata);

            Node thisNode = tree.FindNode(identity);
            MetadataIdentity parentIdentity = identity.Pop();

            if (thisNode != null)
                return thisNode;
            else if (parentIdentity != null)
            {
                Node parentNode = tree.FindNode(parentIdentity) ?? AddDynamicNode(tree, parentIdentity, metadata);

                if (parentNode != null)
                {
                    thisNode = new Node(identity, metadata)
                    {
                        IsDynamic = true,
                    };

                    parentNode.Properties.Add(thisNode);
                    tree.Nodes.Add(thisNode);
                }
            }

            return thisNode;
        }

        private static Node AddStaticNode(NodeTree tree, IBindingMetadata metadata)
        {
            Node thisNode = tree.FindNode(metadata);

            if (thisNode != null)
                return thisNode;
            else if (metadata.HasFlag(BindingMetadataFlags.Item))
            {
                thisNode = new Node(metadata)
                {
                    IsDynamic = metadata.HasFlag(BindingMetadataFlags.Dynamic),
                };

                tree.Nodes.Add(thisNode);
                tree.Items.Add(thisNode);
            }
            else
            {
                Node parentNode = tree.FindNode(metadata.Parent) ?? AddStaticNode(tree, metadata.Parent);

                if (parentNode != null)
                {
                    thisNode = new Node(metadata)
                    {
                        IsDynamic = metadata.HasFlag(BindingMetadataFlags.Dynamic),
                    };

                    parentNode.Properties.Add(thisNode);
                    tree.Nodes.Add(thisNode);
                }
            }

            return thisNode;
        }

        private static bool IsValidMetadata(IBindingMetadata metadata) => (metadata != null && !metadata.MemberOf.HasFlag(BindingMetadataFlags.Model));
        private static IBindingMetadata FindDynamicMetadata(MetadataIdentity identity)
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
