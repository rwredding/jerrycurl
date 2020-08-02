using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Data.Queries.Internal.V11.Writers;
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
                    Item = this.GetBinder(itemNode, valueMap),
                };

                if (writer.Item is NewBinder newbinder)
                {
                    writer.PrimaryKey = newbinder.PrimaryKey;
                    newbinder.PrimaryKey = null;
                }

                this.AddChildKey(writer);

                tree.Lists.Add(writer);
            }
        }

        private NodeBinder GetBinder(Node node, Dictionary<MetadataIdentity, ColumnIdentity> valueMap)
        {
            if (valueMap.TryGetValue(node.Identity, out ColumnIdentity column))
            {
                return new DataBinder()
                {
                    Metadata = node.Metadata,
                    Column = column,
                    CanBeDbNull = true,
                };
            }
            else
            {
                NewBinder binder = new NewBinder()
                {
                    Metadata = node.Metadata,
                    Properties = node.Properties.Select(n => this.GetBinder(n, valueMap)).ToList(),
                    JoinKeys = new List<ValueKey>(),
                };

                this.AddPrimaryKey(binder);
                this.AddParentKeys(binder);

                return binder;
            }
        }

        private void AddPrimaryKey(NewBinder binder)
        {
            IReferenceKey primaryKey = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>()?.Keys.FirstOrDefault(k => k.IsPrimaryKey);
            ValueKey key = KeyHelper.FindKey(binder, primaryKey.Properties);

            if (key != null)
            {
                binder.PrimaryKey = key;

                foreach (ValueBinder value in key.Values)
                    value.CanBeDbNull = false;
            }
        }

        private void AddParentKeys(NewBinder binder)
        {
            IReferenceMetadata metadata = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>();

            foreach (IReference reference in metadata.References.Where(r => r.HasFlag(ReferenceFlags.Parent) && r.Other.HasFlag(ReferenceFlags.Many)))
            {
                ValueKey key = KeyHelper.FindKey(binder, reference.Key.Properties);

                if (key != null)
                {
                    ListBinder listBinder = new ListBinder()
                    {
                        Metadata = reference.Other.Metadata.Identity.GetMetadata<IBindingMetadata>().Parent,
                    };

                    binder.JoinKeys.Add(key);
                    binder.Properties.Add(listBinder);
                }
            }
        }

        private void AddChildKey(ListWriter writer)
        {
            IReferenceMetadata metadata = writer.Item.Metadata.Identity.GetMetadata<IReferenceMetadata>();
            IReferenceKey childKey = metadata.References.FirstOrDefault(r => r.HasFlag(ReferenceFlags.Child | ReferenceFlags.Many))?.Key;

            if (writer.Item is NewBinder binder)
                writer.JoinKey = KeyHelper.FindKey(binder, childKey?.Properties);
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
