using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Nodes;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class ElementNode
    {
        public MetadataNode Value { get; set; }
        public IBindingMetadata List { get; set; }
        public int? ListIndex { get; set; }

        public IList<KeyNode> ChildKeys { get; set; } = new List<KeyNode>();
        public IList<KeyNode> ParentKeys { get; set; } = new List<KeyNode>();
        public IEnumerable<KeyNode> Keys => this.ParentKeys.Concat(this.ChildKeys);

        public override string ToString() => this.Value.ToString();
    }
}
