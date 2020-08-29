using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11.Compilation
{
    internal class BufferWriter
    {
        public Action<RelationBuffer> Initializer { get; set; }
        public Action<RelationBuffer>[] Queues { get; set; }
    }
}
