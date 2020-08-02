using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class BufferParser
    {
        public ISchema Schema { get; }
        public QueryIndexer Indexer { get; }

        public BufferParser(ISchema schema, QueryIndexer indexer)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Indexer = indexer ?? throw new ArgumentException(nameof(indexer));
        }

        public BufferTree Parse(ListIdentity identity)
        {
            NodeParser nodeParser = new NodeParser(this.Schema);
            Dictionary<MetadataIdentity, ColumnIdentity> valueMap = this.GetValueMap(identity);
            NodeTree nodeTree = nodeParser.Parse(valueMap.Keys);
            BufferTree tree = new BufferTree()
            {
                Schema = this.Schema,
            };

            this.MoveManyToOneNodes(nodeTree);
            this.AddWriters(tree, nodeTree, valueMap);

            return tree;
        }

        private void MoveManyToOneNodes(NodeTree nodeTree)
        {
            foreach (Node node in nodeTree.Nodes)
            {
                if (node.Metadata.Annotations.OfType<OneAttribute>().Any())
                {
                    Node parentNode = nodeTree.FindNode(node.Metadata.Parent);

                    parentNode.Properties.Remove(node);

                    nodeTree.Items.Add(node);
                }
            }
        }

        private Dictionary<MetadataIdentity, ColumnIdentity> GetValueMap(ListIdentity identity)
            => identity.Columns.ToDictionary(c => new MetadataIdentity(this.Schema, c.Name));

        private void AddWriters(BufferTree tree, NodeTree nodeTree, Dictionary<MetadataIdentity, ColumnIdentity> valueMap)
        {
            foreach (Node itemNode in nodeTree.Items)
            {
                ListWriter writer = new ListWriter()
                {
                    Metadata = itemNode.Metadata.Parent,
                    Item = this.GetReader(itemNode, valueMap),
                };

                if (writer.Item is NewReader newReader)
                {
                    writer.PrimaryKey = newReader.PrimaryKey;
                    newReader.PrimaryKey = null;
                }

                this.AddChildKey(writer);

                tree.Lists.Add(writer);
            }
        }

        private NodeReader GetReader(Node node, Dictionary<MetadataIdentity, ColumnIdentity> valueMap)
        {
            if (valueMap.TryGetValue(node.Identity, out ColumnIdentity column))
            {
                return new DataReader()
                {
                    Metadata = node.Metadata,
                    Column = column,
                    CanBeDbNull = true,
                };
            }
            else
            {
                NewReader reader = new NewReader()
                {
                    Metadata = node.Metadata,
                    Properties = node.Properties.Select(n => this.GetReader(n, valueMap)).ToList(),
                    JoinKeys = new List<KeyReader>(),
                };

                this.AddPrimaryKeys(reader);
                this.AddParentKeys(reader);

                return reader;
            }
        }

        private void AddPrimaryKeys(NewReader reader)
        {
            IReferenceMetadata metadata = reader.Metadata.Identity.GetMetadata<IReferenceMetadata>();
            IReferenceKey primaryKey = metadata.Keys.FirstOrDefault(k => k.IsPrimaryKey);
            KeyReader key = this.GetKeyReader(reader, primaryKey?.Properties);

            if (key != null)
            {
                reader.PrimaryKey = key;

                foreach (DataReader value in key.Values)
                    value.CanBeDbNull = false;
            }
        }

        private void AddParentKeys(NewReader reader)
        {
            IReferenceMetadata metadata = reader.Metadata.Identity.GetMetadata<IReferenceMetadata>();

            foreach (IReference reference in metadata.References.Where(r => r.HasFlag(ReferenceFlags.Parent) && r.Other.HasFlag(ReferenceFlags.Many)))
            {
                KeyReader key = this.GetKeyReader(reader, reference.Key.Properties);

                if (key != null)
                {
                    ListReader listReader = new ListReader()
                    {
                        Metadata = reference.Other.Metadata.Identity.GetMetadata<IBindingMetadata>().Parent,
                    };

                    reader.JoinKeys.Add(key);
                    reader.Properties.Add(listReader);
                }
            }
        }



        private void AddChildKey(ListWriter writer)
        {
            IReferenceMetadata metadata = writer.Item.Metadata.Identity.GetMetadata<IReferenceMetadata>();
            IReferenceKey childKey = metadata.References.FirstOrDefault(r => r.HasFlag(ReferenceFlags.Child | ReferenceFlags.Many))?.Key;

            if (writer.Item is NewReader newReader)
                writer.JoinKey = this.GetKeyReader(newReader, childKey?.Properties);
        }

        private KeyReader GetKeyReader(NewReader reader, IReadOnlyList<IReferenceMetadata> properties)
        {
            if (properties == null)
                return null;

            List<DataReader> values = new List<DataReader>();

            foreach (IReferenceMetadata metadata in properties)
            {
                DataReader dataReader = reader.Properties.OfType<DataReader>().FirstOrDefault(r => r.Metadata.Identity.Equals(metadata.Identity));

                values.Add(dataReader);
            }

            if (values.Count == properties.Count)
            {
                KeyReader key = new KeyReader()
                {
                    Values = values,
                    Variable = Expression.Variable(this.GetDictionaryType(values.Select(r => r.Metadata.Type))),
                };

                return key;
            }


            return null;
        }

        private Type GetDictionaryType(IEnumerable<Type> keyType)
            => typeof(Dictionary<,>).MakeGenericType(this.GetCompositeKeyType(keyType), typeof(ElasticArray));

        private Type GetCompositeKeyType(IEnumerable<Type> keyType)
        {
            Type[] typeArray = keyType.ToArray();

            if (typeArray.Length == 0)
                return null;
            else if (typeArray.Length == 1)
                return typeof(CompositeKey<>).MakeGenericType(typeArray[0]);
            else if (typeArray.Length == 2)
                return typeof(CompositeKey<,>).MakeGenericType(typeArray[0], typeArray[1]);
            else if (typeArray.Length == 3)
                return typeof(CompositeKey<,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2]);
            else if (typeArray.Length == 4)
                return typeof(CompositeKey<,,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2], typeArray[3]);
            else
            {
                Type restType = this.GetCompositeKeyType(keyType.Skip(4));

                return typeof(CompositeKey<,,,,>).MakeGenericType(typeArray[0], typeArray[1], typeArray[2], typeArray[3], restType);
            }
        }
    }
}
