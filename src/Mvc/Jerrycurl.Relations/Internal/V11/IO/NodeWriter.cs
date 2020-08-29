using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class NodeWriter
    {
        public IRelationMetadata Metadata { get; }
        public string NamePart { get; set; }

        public NodeWriter(Node node)
        {
            this.Metadata = node.Metadata;
        }

        public override string ToString() => this.Metadata.Identity.Name;
    }
}
