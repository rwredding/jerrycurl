using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class FieldWriter : NodeWriter
    {
        public int BufferIndex { get; set; }

        public FieldWriter(Node node)
            : base(node)
        {
            
        }

        public override string ToString() => this.Metadata.Identity.Name;
    }
}
