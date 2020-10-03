using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.V11.Internal.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11.Internal.IO
{
    internal class FieldWriter : NodeWriter
    {
        public int BufferIndex { get; set; }

        public FieldWriter(Node node)
            : base(node)
        {
            
        }
    }
}
