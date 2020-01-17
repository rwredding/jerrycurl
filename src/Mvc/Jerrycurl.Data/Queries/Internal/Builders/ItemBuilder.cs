using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Nodes;

namespace Jerrycurl.Data.Queries.Internal.Builders
{
    internal class ItemBuilder
    {
        private readonly TableIdentity tableInfo;
        private readonly ISchema schema;

        public ItemBuilder(ISchema schema, TableIdentity tableInfo)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.tableInfo = tableInfo ?? throw new ArgumentNullException(nameof(tableInfo));
        }

        public EnumerateNode Build()
        {
            List<MetadataNode> itemNodes = new List<MetadataNode>();

            foreach (ColumnIdentity value in this.tableInfo.Columns)
                this.AddValueNode(itemNodes, value);

            this.AddNullKeys(itemNodes);

            return new EnumerateNode()
            {
                Helpers = this.GetHelperNodes(itemNodes).ToList(),
                Items = itemNodes,
            };
        }

        private IEnumerable<HelperNode> GetHelperNodes(IEnumerable<MetadataNode> itemNodes)
        {
            Dictionary<IBindingHelperContract, HelperNode> indexMap = new Dictionary<IBindingHelperContract, HelperNode>();

            foreach (MetadataNode valueNode in itemNodes.SelectMany(n => n.Tree()).Where(n => n.Column != null && n.Metadata.Helper != null))
            {
                if (!indexMap.TryGetValue(valueNode.Metadata.Helper, out HelperNode helperNode))
                {
                    helperNode = new HelperNode()
                    {
                        Index = indexMap.Count,
                        Type = valueNode.Metadata.Helper.Type,
                        Object = valueNode.Metadata.Helper.Object,
                    };

                    indexMap.Add(valueNode.Metadata.Helper, helperNode);

                    yield return helperNode;
                }

                valueNode.Helper = helperNode;
            }
        }

        private void AddNullKeys(IEnumerable<MetadataNode> itemNodes)
        {
            foreach (MetadataNode itemNode in itemNodes)
            {
                foreach (MetadataNode node in itemNode.Tree())
                {
                    IReferenceMetadata metadata = node.GetMetadata<IReferenceMetadata>();
                    IEnumerable<IReferenceMetadata> properties = metadata?.Properties.Where(m => m.HasFlag(ReferenceMetadataFlags.PrimaryKey)) ?? Array.Empty<IReferenceMetadata>();

                    IList<MetadataNode> keyValues = properties.Select(m => itemNode.FindNode(m)).NotNull().ToList();

                    if (keyValues.Any())
                    {
                        node.NullKey = new KeyNode()
                        {
                            Value = keyValues,
                        };

                        foreach (MetadataNode keyValue in keyValues)
                            keyValue.Flags |= NodeFlags.Key;
                    }
                }
            }
        }

        private void AddValueNode(List<MetadataNode> itemNodes, ColumnIdentity value)
        {
            MetadataIdentity identity = new MetadataIdentity(this.schema, value.Name);
            IBindingMetadata metadata = identity.GetMetadata<IBindingMetadata>() ?? this.FindDynamicMetadata(identity);

            if (metadata != null && !this.IsOutsideResultScope(metadata))
            {
                if (metadata.HasFlag(BindingMetadataFlags.Dynamic))
                    this.AddDynamicNode(itemNodes, identity, metadata, value);
                else
                    this.AddStaticNode(itemNodes, metadata, value);
            }
        }

        private MetadataNode AddDynamicNode(List<MetadataNode> itemNodes, MetadataIdentity identity, IBindingMetadata metadata, ColumnIdentity value)
        {
            this.AddStaticNode(itemNodes, metadata, null);

            MetadataNode thisNode = this.FindNode(itemNodes, identity);

            MetadataIdentity parentIdentity = identity.Parent();

            if (thisNode != null)
                thisNode.Column ??= value;
            else if (parentIdentity != null)
            {
                MetadataNode parentNode = this.FindNode(itemNodes, parentIdentity) ?? this.AddDynamicNode(itemNodes, parentIdentity, metadata, null);

                if (parentNode != null)
                {
                    thisNode = new MetadataNode(identity)
                    {
                        Column = value,
                        Metadata = metadata,
                        Flags = NodeFlags.Dynamic,
                    };

                    parentNode.Properties.Add(thisNode);
                }
            }

            return thisNode;
        }

        private MetadataNode AddStaticNode(List<MetadataNode> itemNodes, IBindingMetadata metadata, ColumnIdentity value)
        {
            MetadataNode thisNode = this.FindNode(itemNodes, metadata);

            if (thisNode != null)
                thisNode.Column ??= value;
            else if (metadata.HasFlag(BindingMetadataFlags.Item))
            {
                MetadataNode itemNode = new MetadataNode(metadata)
                {
                    Metadata = metadata,
                    Column = value,
                    Flags = metadata.HasFlag(BindingMetadataFlags.Dynamic) ? NodeFlags.Dynamic : NodeFlags.None,
                };

                if (metadata.Parent.HasFlag(BindingMetadataFlags.Model))
                    itemNode.Flags |= NodeFlags.Result;

                itemNodes.Add(itemNode);

                return itemNode;
            }
            else
            {
                MetadataNode parentNode = this.FindNode(itemNodes, metadata.Parent) ?? this.AddStaticNode(itemNodes, metadata.Parent, null);

                if (parentNode != null)
                {
                    thisNode = new MetadataNode(metadata)
                    {
                        Metadata = metadata,
                        Column = value,
                        Flags = metadata.HasFlag(BindingMetadataFlags.Dynamic) ? NodeFlags.Dynamic : NodeFlags.None,
                    };

                    parentNode.Properties.Add(thisNode);
                }
            }

            return thisNode;
        }

        private MetadataNode FindNode(IEnumerable<MetadataNode> itemNodes, IBindingMetadata metadata) => itemNodes.NotNull(n => n.FindNode(metadata)).FirstOrDefault();
        private MetadataNode FindNode(IEnumerable<MetadataNode> itemNodes, MetadataIdentity identity) => itemNodes.NotNull(n => n.FindNode(identity)).FirstOrDefault();

        private bool IsOutsideResultScope(IBindingMetadata metadata) => metadata.MemberOf.HasFlag(BindingMetadataFlags.Model);

        private IBindingMetadata FindDynamicMetadata(MetadataIdentity identity)
        {
            IBindingMetadata metadata = identity.GetMetadata<IBindingMetadata>();

            while (metadata == null && (identity = identity.Parent()) != null)
                metadata = identity.GetMetadata<IBindingMetadata>();

            if (metadata != null && metadata.HasFlag(BindingMetadataFlags.Dynamic))
                return metadata;

            return null;
        }
    }
}
