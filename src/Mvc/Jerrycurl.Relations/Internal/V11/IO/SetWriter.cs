using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Parsing;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class SetWriter : NodeWriter
    {
        public int EnumeratorIndex { get; set; }
        public Type EnumeratorType { get; set; }
        public bool IsSource { get; set; }

        public SetWriter(Node node)
            : base(node)
        {

        }
    }
}
