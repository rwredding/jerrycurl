using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Jerrycurl.Relations.Internal.V11.Enumerators;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class BufferParser
    {
        public BufferTree Parse(MetadataIdentity source, RelationIdentity2 relation)
        {
            NodeTree nodeTree = NodeParser.Parse(source, relation);
            BufferTree tree = new BufferTree();

            this.AddReaders(nodeTree, tree);

            return tree;
        }

        private void AddReaders(NodeTree nodeTree, BufferTree tree)
        {
            SetReader reader = new SetReader(nodeTree.Source);

            reader.Properties = nodeTree.Source.Properties.Select(n => this.CreateReader(n, tree)).ToList();

            tree.Sets.Add(reader);
        }

        private ValueReader CreateReader(Node node, BufferTree tree)
        {
            ValueReader reader;

            if (node.Item != null)
            {
                SetWriter writer = new SetWriter(node)
                {
                    EnumeratorIndex = tree.Sets.Count,
                };

                reader = new SetReader(node);

                SetReader setReader = new SetReader(node.Item)
                {
                    EnumeratorIndex = writer.EnumeratorIndex
                };
                setReader.Writers.Add(writer);

                tree.Sets.Add(setReader);

                return setReader;
            }
            else
                reader = new ValueReader(node);
            
            reader.Properties = node.Properties.Select(n => this.CreateReader(n, tree)).ToList();

            this.AddWriters(node, reader, tree);

            return reader;
        }

        private void AddWriters(Node node, ValueReader reader, BufferTree tree)
        {
            foreach (int index in node.Index)
            {
                FieldWriter writer = new FieldWriter(node)
                {
                    BufferIndex = index,
                };

                reader.Writers.Add(writer);

                tree.Fields.Add(writer);
            }
        }


        private string GetNamePart(Node node, NodeTree nodeTree)
        {
            IRelationMetadata parentItem = node.Metadata.Parent?.MemberOf;
            Node parentNode = nodeTree.FindNode(parentItem);

            if (parentNode != null)
                return parentItem.Identity.Schema.Notation.Path(parentItem.Identity.Name, node.Metadata.Identity.Name);

            return node.Metadata.Identity.Name;
        }
    }
}
