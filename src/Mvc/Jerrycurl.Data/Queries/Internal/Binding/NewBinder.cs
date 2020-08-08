using System.Collections.Generic;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Parsing;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class NewBinder : NodeBinder
    {
        public NewBinder(Node node)
            : base(node)
        {
            this.IsDynamic = node.IsDynamic;
        }

        public NewBinder(IBindingMetadata metadata)
            : base(metadata)
        {
            this.IsDynamic = metadata.HasFlag(BindingMetadataFlags.Dynamic);
        }

        public KeyBinder PrimaryKey { get; set; }
        public IList<KeyBinder> JoinKeys { get; } = new List<KeyBinder>();
        public IList<NodeBinder> Properties { get; set; } = new List<NodeBinder>();
        public bool IsDynamic { get; }
    }
}
