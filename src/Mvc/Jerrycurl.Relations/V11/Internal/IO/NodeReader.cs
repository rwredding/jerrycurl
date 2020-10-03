using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.V11.Internal.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11.Internal.IO
{
    internal abstract class NodeReader
    {
        public IRelationMetadata Metadata { get; }
        public IList<PropertyReader> Properties { get; set; }
        public IList<NodeWriter> Writers { get; } = new List<NodeWriter>();
        public QueueIndex Index { get; set; }

        public NodeReader(Node node)
        {
            this.Metadata = node.Metadata;
        }

        public override string ToString() => $"{this.GetType().Name}: {this.Metadata.Identity.Name}";
    }
}
