using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
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
