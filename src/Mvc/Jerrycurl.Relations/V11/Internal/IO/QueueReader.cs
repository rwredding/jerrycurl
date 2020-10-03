using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.V11.Internal.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11.Internal.IO
{
    internal class QueueReader : NodeReader
    {
        public QueueReader(Node node)
            : base(node)
        {
            
        }

        public override string ToString() => this.Metadata.Identity.Name;
    }
}
