using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Queries.Internal.V11.Binding;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class BufferParser
    {
        public ISchema Schema { get; }
        public QueryIndexer Indexer { get; }
        public QueryType Type { get; }

        public BufferParser(ISchema schema, QueryIndexer indexer, QueryType queryType)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Indexer = indexer ?? throw new ArgumentException(nameof(indexer));
            this.Type = queryType;
        }

        public BufferTree Parse(ListIdentity identity)
        {
            NodeParser nodeParser = new NodeParser(this.Schema);
            NodeTree nodeTree = nodeParser.Parse(identity.Columns.Select(c => new MetadataIdentity(this.Schema, c.Name)));
            BufferTree tree = new BufferTree()
            {
                QueryType = this.Type,
                Schema = this.Schema,
            };

            this.MoveManyToOneNodes(nodeTree);

            this.AddWriters(tree, nodeTree, identity);
            this.AddAggregates(tree, nodeTree, identity);

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

        private void AddAggregates(BufferTree tree, NodeTree nodeTree, ListIdentity identity)
        {
            if (this.Type != QueryType.Aggregate)
                return;

            foreach (Node node in nodeTree.Nodes.Where(n => n.Depth == 1))
            {
                DataBinder dataBinder = NodeHelper.FindData(node, identity);

                if (dataBinder != null)
                {
                    AggregateWriter writer = new AggregateWriter()
                    {
                        BufferIndex = this.Indexer.GetAggregateIndex(node.Identity),
                        Data = dataBinder,
                    };

                    tree.Aggregates.Add(writer);
                }
            }
        }

        private void AddWriters(BufferTree tree, NodeTree nodeTree, ListIdentity identity)
        {
            IEnumerable<Node> itemNodes = nodeTree.Items;

            if (this.Type == QueryType.Aggregate)
                itemNodes = itemNodes.Where(n => n.Depth != 1);

            foreach (Node itemNode in itemNodes)
            {
                ListWriter writer = new ListWriter()
                {
                    Metadata = itemNode.Metadata.Parent,
                    Item = this.GetBinder(tree, itemNode, identity),
                    BufferIndex = 0,
                };

                if (writer.Item is NewBinder newBinder)
                {
                    writer.PrimaryKey = newBinder.PrimaryKey;
                    newBinder.PrimaryKey = null;
                }

                if (this.Type == QueryType.Aggregate && itemNode.Depth > 2)
                    this.AddChildKey(writer);
                else if (this.Type == QueryType.List && itemNode.Depth > 1)
                    this.AddChildKey(writer);

                writer.Slot = this.AddSlot(tree, writer.Metadata, writer.JoinKey);

                if (writer.JoinKey != null)
                    writer.BufferIndex = this.Indexer.GetListIndex(writer.Metadata.Identity, writer.JoinKey.Metadata.Key, writer.JoinKey.Metadata.Other.Key);

                tree.Lists.Add(writer);
            }
        }

        private Type GetListType(IBindingMetadata metadata) => metadata.Composition.Construct.Type;

        private ParameterExpression AddSlot(BufferTree tree, IBindingMetadata metadata, KeyBinder joinKey)
        {
            int slotIndex;
            Type variableType;
            if (joinKey == null)
            {
                slotIndex = metadata.Relation.Depth == 0 ? this.Indexer.GetResultIndex() : this.Indexer.GetSlotIndex(metadata.Identity);
                variableType = this.GetListType(metadata);
            }
            else
            {
                IReferenceKey parentKey = joinKey.Metadata.HasFlag(ReferenceFlags.Parent) ? joinKey.Metadata.Key : joinKey.Metadata.Other.Key;

                slotIndex = this.Indexer.GetSlotIndex(metadata.Identity, parentKey);
                variableType = this.GetDictionaryType(joinKey);
            }

            SlotWriter slotWriter = tree.Slots.FirstOrDefault(w => w.BufferIndex == slotIndex);

            if (slotWriter == null)
            {
                slotWriter = new SlotWriter()
                {
                    BufferIndex = slotIndex,
                    Variable = Expression.Variable(variableType),
                    Metadata = metadata,
                };

                tree.Slots.Add(slotWriter);
            }

            return slotWriter.Variable;
        }

        private void AddHelper(BufferTree tree, DataBinder binder)
        {
            if (binder.Metadata.Helper != null)
            {
                HelperWriter writer = new HelperWriter()
                {
                    Object = binder.Metadata.Helper.Object,
                    BufferIndex = tree.Helpers.Count,
                    Variable = Expression.Variable(binder.Metadata.Helper.Type),
                };

                binder.Helper = writer.Variable;

                tree.Helpers.Add(writer);
            }
        }

        private NodeBinder GetBinder(BufferTree tree, Node node, ListIdentity identity)
        {
            DataBinder dataBinder = NodeHelper.FindData(node, identity);

            if (dataBinder != null)
            {
                this.AddHelper(tree, dataBinder);

                return dataBinder;
            }
                
            NewBinder binder = new NewBinder()
            {
                Metadata = node.Metadata,
                Properties = node.Properties.Select(n => this.GetBinder(tree, n, identity)).ToList(),
                JoinKeys = new List<KeyBinder>(),
            };

            this.AddPrimaryKey(binder);

            if (this.Type == QueryType.List && node.Depth > 1)
                this.AddParentKeys(tree, binder);
            else if (this.Type == QueryType.Aggregate && node.Depth > 2)
                this.AddParentKeys(tree, binder);

            return binder;
        }

        private void AddPrimaryKey(NewBinder binder)
        {
            IReferenceKey primaryKey = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>()?.Keys.FirstOrDefault(k => k.IsPrimaryKey);
            KeyBinder key = NodeHelper.FindKey(binder, primaryKey);

            if (key != null)
            {
                binder.PrimaryKey = key;

                foreach (ValueBinder valueBinder in key.Values)
                    valueBinder.CanBeDbNull = false;
            }
        }

        private void InitializeKeyVariables(KeyBinder key)
        {
            foreach (ValueBinder value in key.Values)
            {
                value.IsDbNull ??= Expression.Variable(typeof(bool));
                value.Value ??= Expression.Variable(value.Metadata.Type);
            }
        }
        private void AddParentKeys(BufferTree tree, NewBinder binder)
        {
            IReferenceMetadata metadata = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>();

            foreach (IReference reference in metadata.References.Where(r => r.HasFlag(ReferenceFlags.Parent) && r.Other.HasFlag(ReferenceFlags.Many)))
            {
                KeyBinder key = NodeHelper.FindKey(binder, reference.Key);

                if (key != null)
                {
                    IBindingMetadata joinMetadata = reference.Other.Metadata.Identity.GetMetadata<IBindingMetadata>().Parent;

                    JoinBinder joinBinder = new JoinBinder()
                    {
                        Metadata = joinMetadata,
                        Array = Expression.Variable(typeof(ElasticArray)),
                        BufferIndex = this.Indexer.GetListIndex(joinMetadata.Identity, reference.Key, reference.Other.Key),
                    };

                    binder.JoinKeys.Add(key);
                    binder.Properties.Add(joinBinder);

                    this.InitializeKeyVariables(key);
                    this.AddSlot(tree, joinMetadata, key);
                }
            }
        }

        private void AddChildKey(ListWriter writer)
        {
            IReferenceMetadata metadata = writer.Item.Metadata.Identity.GetMetadata<IReferenceMetadata>();
            IReferenceKey childKey = metadata.References.FirstOrDefault(r => r.HasFlag(ReferenceFlags.Child | ReferenceFlags.Many))?.Key;

            if (writer.Item is NewBinder binder)
            {
                writer.JoinKey = NodeHelper.FindKey(binder, childKey);

                if (writer.JoinKey != null)
                {
                    this.InitializeKeyVariables(writer.JoinKey);

                    writer.JoinKey.Variable = Expression.Variable(this.GetCompositeKeyType(writer.JoinKey.Values.Select(v => v.Metadata.Type)));
                }
            }
                
        }

        private Type GetDictionaryType(KeyBinder key)
            => this.GetDictionaryType(key.Values.Select(v => v.Metadata.Type));

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
