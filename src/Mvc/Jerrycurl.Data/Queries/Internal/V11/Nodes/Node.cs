using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    internal abstract class Node
    {
        public MetadataIdentity Identity { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public NodeFlags Flags { get; set; }
    }
}
