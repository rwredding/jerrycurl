using System;
using System.Collections.Generic;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal class Node
    {
        public Node(MetadataIdentity identity)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            this.Metadata = identity.GetMetadata<IBindingMetadata>() ?? throw new InvalidOperationException("Metadata not found.");
        }

        public Node(IBindingMetadata metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.Identity = metadata?.Identity;
        }

        public Node(MetadataIdentity identity, IBindingMetadata metadata)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public MetadataIdentity Identity { get; }
        public IBindingMetadata Metadata { get; }
        public IList<Node> Properties { get; } = new List<Node>();
        public bool IsDynamic { get; set; }
        public int Depth => this.Metadata.Relation.Depth;
    }
}
