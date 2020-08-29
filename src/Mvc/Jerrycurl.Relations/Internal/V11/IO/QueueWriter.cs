using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Enumerators;
using Jerrycurl.Relations.Internal.V11.Parsing;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class QueueWriter : NodeWriter
    {
        public QueueIndex Next { get; set; }

        public QueueWriter(Node node)
            : base(node)
        {

        }
    }
}
