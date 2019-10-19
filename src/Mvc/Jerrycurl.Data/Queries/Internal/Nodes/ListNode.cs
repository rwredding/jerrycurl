using Jerrycurl.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class ListNode
    {
        public KeyNode ParentKey { get; set; }
        public IList<Type> KeyType { get; set; }
        public int? Index { get; set; }
        public IBindingMetadata Metadata { get; set; }

        public override string ToString() => (this.ParentKey != null ? "Dictionary: " : "List: ") + this.Metadata.Identity.ToString();
    }
}
