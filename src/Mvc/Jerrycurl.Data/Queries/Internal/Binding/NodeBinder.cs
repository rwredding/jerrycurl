using System.Diagnostics;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    [DebuggerDisplay("{GetType().Name,nq}: {Metadata.Identity.ToString(),nq}")]
    internal abstract class NodeBinder
    {
        public IBindingMetadata Metadata { get; protected set; }
        public MetadataIdentity Identity { get; protected set; }

        public NodeBinder(Node node)
        {
            this.Metadata = node.Metadata;
            this.Identity = node.Identity;
        }

        public NodeBinder(IBindingMetadata metadata)
        {
            this.Metadata = metadata;
            this.Identity = metadata.Identity;
        }
    }
}
