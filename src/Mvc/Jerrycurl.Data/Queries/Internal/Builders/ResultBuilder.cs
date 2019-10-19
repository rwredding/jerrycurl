using Jerrycurl.Collections;
using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Queries.Internal.Nodes;
using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Builders
{
    internal class ResultBuilder
    {
        public TableIdentity Table { get; }
        public TypeState State { get; }
        public ISchema Schema => this.State.Schema;

        public ResultBuilder(TypeState state, TableIdentity table)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
            this.Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public ResultNode Build()
        {
            this.PreserveIndices();

            ItemBuilder itemBuilder = new ItemBuilder(this.Schema, this.Table);
            EnumerateNode enumerateNode = itemBuilder.Build();

            IList<MetadataNode> itemNodes = this.GetItemNodes(enumerateNode).ToList();
            IList<ElementNode> elementNodes = this.GetElementNodes(itemNodes).ToList();
            IList<ListNode> listNodes = this.GetListNodes(elementNodes).ToList();
            IList<ListNode> dictNodes = this.GetDictionaryNodes(elementNodes).ToList();

            this.AddAggregateTargets(elementNodes);

            return new ResultNode()
            {
                Lists = listNodes.Concat(dictNodes).ToList(),
                Elements = elementNodes,
                Helpers = enumerateNode.Helpers,
            };
        }

        private void PreserveIndices()
        {
            IBindingMetadata resultMetadata = this.Schema.GetMetadata<IBindingMetadata>().Item;

            this.GetListIndex(resultMetadata.Identity);
        }

        private IEnumerable<MetadataNode> GetItemNodes(EnumerateNode enumerateNode)
        {
            List<MetadataNode> itemNodes = enumerateNode.Items.ToList();
            List<MetadataNode> allNodes = itemNodes.SelectMany(n => n.Tree()).Except(itemNodes).ToList();
            List<MetadataNode> fakeNodes = allNodes.Where(n => this.HasManyAttribute(n.Metadata)).ToList();

            foreach (MetadataNode itemNode in fakeNodes)
            {
                MetadataNode parentNode = this.FindNode(itemNodes, itemNode.Metadata.Parent);

                if (parentNode != null)
                {
                    parentNode.Properties.Remove(itemNode);
                    itemNodes.Insert(0, itemNode);
                }
            }

            foreach (MetadataNode itemNode in itemNodes.Where(this.HasValue))
                yield return itemNode;
        }

        private IEnumerable<ElementNode> GetElementNodes(IEnumerable<MetadataNode> itemNodes)
        {
            foreach (MetadataNode itemNode in itemNodes)
            {
                ElementNode elementNode = new ElementNode()
                {
                    ChildKeys = this.GetChildKeys(itemNode).ToList(),
                    ParentKeys = this.GetParentKeys(itemNode).ToList(),
                    Value = itemNode,
                };

                if (itemNode.Metadata.HasFlag(BindingMetadataFlags.Item))
                    elementNode.List = itemNode.Metadata.Parent;

                if (!elementNode.ChildKeys.Any())
                    elementNode.ListIndex = this.GetListIndex(itemNode.Identity);

                this.AssignEmptyChildKeys(elementNode);

                yield return elementNode;
            }
        }

        private IEnumerable<ListNode> GetListNodes(IEnumerable<ElementNode> elementNodes)
        {
            foreach (ElementNode elementNode in elementNodes.Where(n => !n.ChildKeys.Any()))
            {
                yield return new ListNode()
                {
                    Index = this.GetListIndex(elementNode.Value.Identity),
                    Metadata = elementNode.Value.Metadata,
                };
            }
        }

        private IEnumerable<ListNode> GetDictionaryNodes(IEnumerable<ElementNode> elementNodes)
        {
            foreach (var g in elementNodes.SelectMany(n => n.Keys).GroupBy(k => k.ParentIndex))
            {
                KeyNode key = g.First();
                KeyNode parentKey = g.FirstOrDefault(n => n.ChildIndex == null);

                yield return new ListNode()
                {
                    ParentKey = parentKey,
                    KeyType = key.Type,
                    Metadata = key.Metadata,
                    Index = key.ParentIndex,
                };
            }
        }

        private void AssignEmptyChildKeys(ElementNode elementNode)
        {
            IList<MetadataNode> allNodes = elementNode.Value.Tree().ToList();

            foreach (KeyNode parentKey in elementNode.ParentKeys)
            {
                IReference childReference = parentKey.Reference.Other;
                IBindingMetadata childMetadata;

                if (childReference.HasFlag(ReferenceFlags.Many))
                    childMetadata = childReference.List.Identity.GetMetadata<IBindingMetadata>();
                else
                    childMetadata = childReference.Metadata.Identity.GetMetadata<IBindingMetadata>();

                MetadataNode thisNode = this.FindNode(allNodes, childMetadata);
                MetadataNode parentNode = this.FindNode(allNodes, childMetadata?.Parent);

                if (thisNode == null && parentNode != null && parentNode.Column == null)
                {
                    MetadataNode joinNode = new MetadataNode(childMetadata)
                    {
                        ListIndex = parentKey.ParentIndex,
                        ElementIndex = this.GetKeyIndex(parentKey.Reference.Other),
                    };

                    parentNode.Properties.Add(joinNode);
                }
            }
        }

        private void AddAggregateTargets(IEnumerable<ElementNode> elementNodes)
        {
            IBindingMetadata resultItem = this.Schema.GetMetadata<IBindingMetadata>().Item;

            foreach (ElementNode elementNode in elementNodes.Where(n => n.ListIndex != null && n.Value.Metadata.Parent.MemberOf.Equals(resultItem)))
                this.State.Aggregate.Add(elementNode.Value.Metadata.Identity);
        }

        private IEnumerable<KeyNode> GetParentKeys(MetadataNode itemNode)
        {
            IList<MetadataNode> allNodes = itemNode.Tree().ToList();

            foreach (MetadataNode node in allNodes)
            {
                IReferenceMetadata metadata = node.GetMetadata<IReferenceMetadata>();
                IEnumerable<IReference> references = metadata?.References.Where(r => r.HasFlag(ReferenceFlags.Parent)) ?? Array.Empty<IReference>();

                foreach (IReference reference in references.Where(this.IsValidJoinReference))
                {
                    KeyNode keyNode = this.CreateKey(allNodes, reference);

                    if (keyNode != null)
                        yield return keyNode;
                }
            }
        }

        private IEnumerable<KeyNode> GetChildKeys(MetadataNode itemNode)
        {
            IList<MetadataNode> allNodes = itemNode.Tree().ToList();

            IReferenceMetadata metadata = itemNode.GetMetadata<IReferenceMetadata>();
            IEnumerable<IReference> references = metadata?.References.Where(r => r.HasFlag(ReferenceFlags.Child)) ?? Array.Empty<IReference>();

            foreach (IReference reference in references.Where(this.IsValidJoinReference))
            {
                KeyNode childKey = this.CreateKey(allNodes, reference);

                if (childKey != null)
                    yield return childKey;
            }
        }

        private MetadataNode FindNode(IEnumerable<MetadataNode> itemNodes, IMetadata metadata) => itemNodes.NotNull(n => n.FindNode(metadata)).FirstOrDefault();
        private bool HasValue(MetadataNode itemNode) => itemNode.Tree().Any(n => n.Column != null);
        private bool HasManyAttribute(IReference reference) => this.HasManyAttribute(reference.Metadata.Relation);
        private bool HasManyAttribute(IBindingMetadata metadata) => this.HasManyAttribute(metadata.Relation);

        public bool HasManyAttribute(IRelationMetadata metadata)
        {
            if (metadata.Annotations.OfType<ManyAttribute>().Any())
                return true;
            else if (metadata.Parent != null && metadata.Parent.Annotations.OfType<AggregateAttribute>().Any())
                return true;

            return false;
        }

        private bool IsValidJoinReference(IReference reference)
        {
            IReference childReference = reference.HasFlag(ReferenceFlags.Child) ? reference : reference.Other;

            return (childReference.HasFlag(ReferenceFlags.Many) || this.HasManyAttribute(childReference));
        }

        private KeyNode CreateKey(IEnumerable<MetadataNode> nodes, IReference reference)
        {
            IList<MetadataNode> keyValue = reference.Key.Properties.Select(m => this.FindNode(nodes, m)).ToList();

            if (keyValue.All(n => n?.Column != null))
            {
                IReference parentReference = reference.HasFlag(ReferenceFlags.Parent) ? reference : reference.Other;

                KeyNode keyNode = new KeyNode(reference)
                {
                    Value = keyValue,
                    ParentIndex = this.GetKeyIndex(parentReference),
                };

                if (reference.HasFlag(ReferenceFlags.Child))
                    keyNode.ChildIndex = this.GetKeyIndex(parentReference.Other);

                return keyNode;
            }

            return null;
        }

        private int GetListIndex(MetadataIdentity identity) => this.State.Indexer.GetListIndex(identity);
        private int GetKeyIndex(IReference reference) => this.State.Indexer.GetKeyIndex(reference);
    }
}
