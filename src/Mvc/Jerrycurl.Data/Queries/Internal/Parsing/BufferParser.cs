using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Queries.Internal.Binding;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Data.Queries.Internal.Extensions;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal class BufferParser
    {
        public ISchema Schema => this.Buffer.Schema;
        public BufferCache Buffer { get; }
        public QueryType Type { get; }

        public BufferParser(QueryType type, BufferCache cache)
        {
            this.Type = type;
            this.Buffer = cache ?? throw new ArgumentException(nameof(cache));
        }

        public BufferTree Parse(IEnumerable<ColumnName> valueNames)
        {
            NodeTree nodeTree = NodeParser.Parse(this.Schema, valueNames);
            BufferTree tree = new BufferTree()
            {
                QueryType = this.Type,
                Schema = this.Schema,
            };

            this.MoveManyToOneNodes(nodeTree);

            this.AddWriters(tree, nodeTree, valueNames);
            this.AddAggregates(tree, nodeTree, valueNames);
            this.PrioritizeWriters(tree);

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

        private void PrioritizeWriters(BufferTree tree)
        {
            foreach (ListWriter writer in tree.Lists)
                writer.Priority = -writer.Metadata.Relation.Depth;

            int priorityBump = 0;

            foreach (ListWriter writer in tree.Lists.OrderByDescending(w => w.Priority))
            {
                if (writer.IsOneToMany)
                    priorityBump++;

                writer.Priority -= priorityBump;
            }
        }

        private void AddAggregates(BufferTree tree, NodeTree nodeTree, IEnumerable<ColumnName> valueNames)
        {
            foreach (Node node in nodeTree.Nodes.Where(n => this.IsAggregateSet(n.Metadata)))
            {
                ColumnBinder value = BindingHelper.FindValue(node, valueNames);

                if (value != null)
                {
                    AggregateWriter writer = new AggregateWriter()
                    {
                        BufferIndex = this.Buffer.GetAggregateIndex(node.Identity),
                        Data = value,
                    };

                    AggregateName name = new AggregateName(value.Identity.Name, isPrincipal: false);

                    tree.Aggregates.Add(writer);
                    tree.AggregateNames.Add(name);
                }
            }
        }

        private void AddWriters(BufferTree tree, NodeTree nodeTree, IEnumerable<ColumnName> valueNames)
        {
            IEnumerable<Node> itemNodes = nodeTree.Items.Where(n => !this.IsAggregateSet(n.Metadata));

            foreach (Node itemNode in itemNodes)
            {
                ListWriter writer = new ListWriter()
                {
                    Metadata = itemNode.Metadata.Parent,
                    Item = this.CreateBinder(tree, itemNode, valueNames),
                };

                if (writer.Item is NewBinder newBinder)
                {
                    writer.PrimaryKey = newBinder.PrimaryKey;
                    newBinder.PrimaryKey = null;
                }

                if (!this.IsPrincipalSet(itemNode.Metadata))
                    this.AddChildKey(writer);
                else if (this.IsAggregateSet(writer.Metadata))
                    tree.AggregateNames.Add(new AggregateName(writer.Metadata.Identity.Name, isPrincipal: true));

                writer.Slot = this.AddSlot(tree, writer.Metadata, writer.JoinKey);

                if (writer.JoinKey != null)
                {
                    writer.BufferIndex = this.Buffer.GetChildIndex(writer.JoinKey.Metadata);
                    writer.IsOneToMany = this.HasOneAttribute(writer.JoinKey.Metadata);
                }
                    

                tree.Lists.Add(writer);
            }
        }

        private bool IsPrincipalSet(IBindingMetadata metadata)
        {
            bool isPrincipal = metadata.Relation.Depth == (this.Type == QueryType.Aggregate ? 2 : 1);

            return (isPrincipal && !this.HasOneAttribute(metadata));
        }

        private bool IsAggregateSet(IBindingMetadata metadata) => (this.Type == QueryType.Aggregate && metadata.Relation.Depth == 1);

        private ParameterExpression AddSlot(BufferTree tree, IBindingMetadata metadata, KeyBinder joinKey)
        {
            int bufferIndex;
            Type variableType;
            if (joinKey == null)
            {
                bufferIndex = metadata.Relation.Depth == 0 ? this.Buffer.GetResultIndex() : this.Buffer.GetListIndex(metadata.Identity);
                variableType = metadata.Composition.Construct.Type;
            }
            else
            {
                bufferIndex = this.Buffer.GetParentIndex(joinKey.Metadata);
                variableType = this.GetDictionaryType(joinKey);
            }

            SlotWriter slotWriter = tree.Slots.FirstOrDefault(w => w.BufferIndex == bufferIndex);

            if (slotWriter == null)
            {
                slotWriter = new SlotWriter()
                {
                    BufferIndex = bufferIndex,
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

        private NodeBinder CreateBinder(BufferTree tree, Node node, IEnumerable<ColumnName> valueNames)
        {
            ColumnBinder columnBinder = BindingHelper.FindValue(node, valueNames);

            if (columnBinder != null)
            {
                this.AddHelper(tree, columnBinder);

                return columnBinder;
            }

            NewBinder binder = new NewBinder(node)
            {
                Properties = node.Properties.Select(n => this.CreateBinder(tree, n, valueNames)).ToList(),
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
            IReferenceMetadata newMetadata = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>();

            foreach (IReference reference in newMetadata.References.Where(r => r.HasFlag(ReferenceFlags.Parent) && this.IsValidReference(r)))
            {
                KeyBinder joinKey = BindingHelper.FindParentKey(binder, reference);

                if (joinKey != null)
                {
                    IBindingMetadata metadata = (reference.List ?? reference.Other.Metadata).To<IBindingMetadata>();

                    JoinBinder joinBinder = new JoinBinder(metadata)
                    {
                        Array = joinKey.Array ??= BindingHelper.Variable(typeof(ElasticArray), metadata.Identity),
                        ArrayIndex = this.Buffer.GetChildIndex(reference),
                        IsManyToOne = reference.Other.HasFlag(ReferenceFlags.One),
                    };

                    binder.JoinKeys.Add(joinKey);
                    binder.Properties.Add(joinBinder);

                    this.InitializeKeyVariables(joinKey);
                    this.AddSlot(tree, metadata, joinKey);
                }
            }
        }

        private void AddChildKey(ListWriter writer)
        {
            if (writer.Item is NewBinder binder)
            {
                IReferenceMetadata metadata = writer.Item.Metadata.To<IReferenceMetadata>();
                IReference reference = metadata.References.FirstOrDefault(r => r.HasFlag(ReferenceFlags.Child) && this.IsValidReference(r));

                KeyBinder joinKey = writer.JoinKey = BindingHelper.FindChildKey(binder, reference);

                if (joinKey != null)
                {
                    this.InitializeKeyVariables(joinKey);

                    joinKey.Array ??= BindingHelper.Variable(typeof(ElasticArray), joinKey.Metadata.Metadata.Identity);
                }
            }
        }

        private bool IsValidReference(IReference reference)
        {
            IReference childReference = reference.Find(ReferenceFlags.Child);

            if (childReference.HasFlag(ReferenceFlags.Self) && childReference.HasFlag(ReferenceFlags.Primary))
                return false;

            return (childReference.HasFlag(ReferenceFlags.Many) || this.HasOneAttribute(childReference));
        }

        private Type GetDictionaryType(KeyBinder key)
            => typeof(Dictionary<,>).MakeGenericType(key.KeyType, typeof(ElasticArray));
    }
}
