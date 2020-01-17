using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class AggregateNode
    {
        public MetadataNode Item { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public int Index { get; set; }
    }
}
