using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.V11.Internal.Enumerators;
using Jerrycurl.Relations.V11.Internal.Parsing;

namespace Jerrycurl.Relations.V11.Internal.IO
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
