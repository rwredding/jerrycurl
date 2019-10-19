using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class MetadataNode
    {
        public MetadataIdentity Identity { get; }
        public NodeFlags Flags { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public IList<MetadataNode> Properties { get; set; } = new List<MetadataNode>();
        public ColumnIdentity Column { get; set; }
        public KeyNode NullKey { get; set; }
        //public int? HelperIndex { get; set; }
        public HelperNode Helper { get; set; }
        public IList<Type> KeyTypes { get; set; } = new List<Type>();

        public int? ListIndex { get; set; }
        public int? ElementIndex { get; set; }

        public MetadataNode(MetadataIdentity identity)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public MetadataNode(IBindingMetadata metadata)
            : this(metadata?.Identity)
        {
            this.Metadata = metadata;
        }

        public virtual IReadOnlyList<MetadataNode> Tree()
        {
            IEnumerable<MetadataNode> iterator(MetadataNode node)
            {
                yield return node;

                foreach (MetadataNode node2 in node.Properties)
                    foreach (MetadataNode node3 in iterator(node2))
                        yield return node3;
            }

            return iterator(this).ToList();
        }

        public MetadataNode FindNode(MetadataIdentity identity) => this.Tree().FirstOrDefault(n => n.Identity.Equals(identity));
        public MetadataNode FindNode(IMetadata metadata) => this.FindNode(metadata.Identity);
        public MetadataNode FindNode(string name) => this.FindNode(new MetadataIdentity(this.Identity.Schema, name));

        public TMetadata GetMetadata<TMetadata>()
            where TMetadata : class, IMetadata
            => this.Identity?.GetMetadata<TMetadata>();

        public bool HasFlag(NodeFlags flags) => this.Flags.HasFlag(flags);

        public override string ToString() => this.Identity.ToString();
    }
}
