using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class ValueReader
    {
        public IRelationMetadata Metadata { get; }
        public IList<ValueReader> Properties { get; set; }
        public IList<NodeWriter> Writers { get; } = new List<NodeWriter>();

        public ValueReader(Node node)
        {
            this.Metadata = node.Metadata;
        }

        public override string ToString() => this.Metadata.Identity.Name;
    }
}
