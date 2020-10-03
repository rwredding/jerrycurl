using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11.Internal.Parsing
{
    internal static class NodeParser
    {
        public static NodeTree Parse(MetadataIdentity source, RelationHeader header)
        {
            NodeTree tree = new NodeTree();

            IRelationMetadata sourceMetadata = GetMetadata(source);
            IRelationMetadata[] relationMetadata = header.Attributes.Select(a => a.Metadata).ToArray();

            tree.Source = AddSourceNode(tree, sourceMetadata);

            for (int i = 0; i < relationMetadata.Length; i++)
                AddNode(tree, relationMetadata[i], index: i);

            return tree;
        }

        private static IRelationMetadata GetMetadata(MetadataIdentity identity)
        {
            IRelationMetadata metadata = identity.GetMetadata<IRelationMetadata>();

            if (metadata == null)
                throw new InvalidOperationException(identity.Name + " not found");

            return metadata;
        }

        private static Node AddSourceNode(NodeTree tree, IRelationMetadata metadata)
        {
            Node sourceNode = new Node(metadata);

            tree.Nodes.Add(sourceNode);

            return sourceNode;
        }
        private static Node AddNode(NodeTree tree, IRelationMetadata metadata, int? index = null)
        {
            Node thisNode = tree.FindNode(metadata);

            if (thisNode == null && metadata.Parent == null)
                return null;
            else if (thisNode == null)
            {
                Node parentNode = tree.FindNode(metadata.Parent) ?? AddNode(tree, metadata.Parent);

                if (parentNode == null)
                    tree.Invalid.Add(metadata);
                else
                {
                    thisNode = new Node(metadata);

                    if (thisNode.Metadata.HasFlag(RelationMetadataFlags.Item))
                        parentNode.Item = thisNode;
                    else
                        parentNode.Properties.Add(thisNode);

                    tree.Nodes.Add(thisNode);
                }
            }

            if (thisNode != null && index != null)
                thisNode.Index.Add(index.Value);

            return thisNode;
        }
    }
}
