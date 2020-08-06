using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Queries.Internal.V11.Binding;
using Jerrycurl.Data.Queries.Internal.V11.Caching;
using Jerrycurl.Data.Queries.Internal.V11.Extensions;
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

        public BufferTree Parse(IEnumerable<ColumnValue> values)
        {
            NodeTree nodeTree = NodeParser.Parse(this.Schema, values);
            BufferTree tree = new BufferTree()
            {
                QueryType = this.Type,
                Schema = this.Schema,
            };

            this.MoveManyToOneNodes(nodeTree);

            this.AddWriters(tree, nodeTree, values);
            this.AddAggregates(tree, nodeTree, values);

            return tree;
        }

        private void MoveManyToOneNodes(NodeTree nodeTree)
        {
            foreach (Node node in nodeTree.Nodes.Where(n => this.HasOneAttribute(n.Metadata)))
            {
                Node parentNode = nodeTree.FindNode(node.Metadata.Parent);

                parentNode.Properties.Remove(node);

                nodeTree.Items.Add(node);
            }
        }

        private bool HasOneAttribute(IBindingMetadata metadata) => metadata.Annotations.OfType<OneAttribute>().Any();
        private bool HasOneAttribute(IReference reference) => reference.Metadata.Annotations.OfType<OneAttribute>().Any();

        private void AddAggregates(BufferTree tree, NodeTree nodeTree, IEnumerable<ColumnValue> values)
        {
            if (this.Type != QueryType.Aggregate)
                return;

            foreach (Node node in nodeTree.Nodes.Where(n => n.Depth == 1))
            {
                ColumnBinder binder = BindingHelper.FindValue(node, values);

                if (binder != null)
                {
                    AggregateWriter writer = new AggregateWriter()
                    {
                        BufferIndex = this.Indexer.GetAggregateIndex(node.Identity),
                        Data = binder,
                    };

                    AggregateValue x = new AggregateValue(binder.Metadata.Identity.Name, isPrincipal: false);

                    tree.Aggregates.Add(writer);
                    tree.Xs.Add(x);
                }
            }
        }

        private void AddWriters(BufferTree tree, NodeTree nodeTree, IEnumerable<ColumnValue> values)
        {
            IEnumerable<Node> itemNodes = this.Type == QueryType.List ? nodeTree.Items : nodeTree.Items.Where(n => n.Depth > 1);

            foreach (Node itemNode in itemNodes)
            {
                ListWriter writer = new ListWriter()
                {
                    Metadata = itemNode.Metadata.Parent,
                    Item = this.GetBinder(tree, itemNode, values),
                };

                if (writer.Item is NewBinder newBinder)
                {
                    writer.PrimaryKey = newBinder.PrimaryKey;
                    newBinder.PrimaryKey = null;
                }

                if (!this.IsPrincipalList(itemNode.Metadata))
                    this.AddChildKey(writer);
                else if (this.Type == QueryType.Aggregate)
                    tree.Xs.Add(new AggregateValue(writer.Metadata.Identity.Name, isPrincipal: true));

                writer.Slot = this.AddSlot(tree, writer.Metadata, writer.JoinKey);

                if (writer.JoinKey != null)
                {
                    writer.BufferIndex = this.Indexer.GetChildIndex(writer.JoinKey.Metadata);
                    writer.IsOneToMany = this.HasOneAttribute(writer.JoinKey.Metadata);
                }
                    

                tree.Lists.Add(writer);
            }
        }

        private bool IsPrincipalList(IBindingMetadata metadata)
        {
            bool isPrincipal = metadata.Relation.Depth == (this.Type == QueryType.Aggregate ? 2 : 1);

            return (isPrincipal && !this.HasOneAttribute(metadata));
        }

        private ParameterExpression AddSlot(BufferTree tree, IBindingMetadata metadata, KeyBinder joinKey)
        {
            int slotIndex;
            Type variableType;
            if (joinKey == null)
            {
                slotIndex = metadata.Relation.Depth == 0 ? this.Indexer.GetResultIndex() : this.Indexer.GetListIndex(metadata.Identity);
                variableType = metadata.Composition.Construct.Type;
            }
            else
            {
                slotIndex = this.Indexer.GetParentIndex(joinKey.Metadata);
                variableType = this.GetDictionaryType(joinKey);
            }

            SlotWriter slotWriter = tree.Slots.FirstOrDefault(w => w.BufferIndex == slotIndex);

            if (slotWriter == null)
            {
                slotWriter = new SlotWriter()
                {
                    BufferIndex = slotIndex,
                    Variable = BindingHelper.Variable(variableType, metadata.Identity),
                    Metadata = metadata,
                    KeyType = joinKey?.KeyType,
                };

                tree.Slots.Add(slotWriter);
            }

            if (joinKey != null)
                joinKey.Slot = slotWriter.Variable;

            return slotWriter.Variable;
        }

        private void AddHelper(BufferTree tree, ColumnBinder binder)
        {
            if (binder.Metadata.Helper != null)
            {
                HelperWriter writer = new HelperWriter()
                {
                    Object = binder.Metadata.Helper.Object,
                    BufferIndex = tree.Helpers.Count,
                    Variable = BindingHelper.Variable(binder.Metadata.Helper.Type, binder),
                };

                binder.Helper = writer.Variable;

                tree.Helpers.Add(writer);
            }
        }

        private NodeBinder GetBinder(BufferTree tree, Node node, IEnumerable<ColumnIdentity> valueSpan)
        {
            ColumnBinder dataBinder = BindingHelper.FindData(node, valueSpan);

            if (dataBinder != null)
            {
                this.AddHelper(tree, dataBinder);

                return dataBinder;
            }
                
            NewBinder binder = new NewBinder()
            {
                Metadata = node.Metadata,
                Properties = node.Properties.Select(n => this.GetBinder(tree, n, valueSpan)).ToList(),
                JoinKeys = new List<KeyBinder>(),
            };

            BindingHelper.AddPrimaryKey(binder);

            this.AddParentKeys(tree, binder);

            return binder;
        }

        private void InitializeKeyVariables(KeyBinder key)
        {
            foreach (ValueBinder value in key.Values)
            {
                value.IsDbNull ??= BindingHelper.Variable(typeof(bool), value);
                value.Variable ??= BindingHelper.Variable(value.KeyType, value);
            }
        }

        private void AddParentKeys(BufferTree tree, NewBinder binder)
        {
            IReferenceMetadata metadata = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>();

            foreach (IReference reference in metadata.References.Where(this.IsValidReference))
            {
                KeyBinder key = BindingHelper.FindParentKey(binder, reference);

                if (key != null)
                {
                    IBindingMetadata childMetadata = reference.Other.Metadata.To<IBindingMetadata>();

                    JoinBinder joinBinder = new JoinBinder()
                    {
                        Metadata = childMetadata,
                        Array = key.Array ??= BindingHelper.Variable(typeof(ElasticArray), childMetadata.Identity),
                        ArrayIndex = this.Indexer.GetChildIndex(reference),
                        IsManyToOne = this.HasOneAttribute(childMetadata),
                    };

                    if (!joinBinder.IsManyToOne)
                        joinBinder.Metadata = joinBinder.Metadata.Parent;

                    binder.JoinKeys.Add(key);
                    binder.Properties.Add(joinBinder);

                    this.InitializeKeyVariables(key);
                    this.AddSlot(tree, childMetadata, key);
                }
            }
        }

        private void AddChildKey(ListWriter writer)
        {
            if (writer.Item is NewBinder binder)
            {
                IReferenceMetadata metadata = writer.Item.Metadata.To<IReferenceMetadata>();
                IReference reference = metadata.References.FirstOrDefault(this.IsValidReference);

                writer.JoinKey = BindingHelper.FindChildKey(binder, reference);

                if (writer.JoinKey != null)
                {
                    this.InitializeKeyVariables(writer.JoinKey);

                    writer.JoinKey.Array ??= BindingHelper.Variable(typeof(ElasticArray), writer.JoinKey.Metadata.Metadata.Identity);
                }
            }
        }

        private bool IsValidReference(IReference reference)
        {
            IReference childReference = reference.HasFlag(ReferenceFlags.Child) ? reference : reference.Other;

            return (childReference.HasFlag(ReferenceFlags.Many) || this.HasOneAttribute(childReference));
        }

        private Type GetDictionaryType(KeyBinder key)
            => typeof(Dictionary<,>).MakeGenericType(key.KeyType, typeof(ElasticArray));
    }
}
