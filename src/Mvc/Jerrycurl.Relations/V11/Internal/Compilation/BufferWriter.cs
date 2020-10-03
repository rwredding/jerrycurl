using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11.Internal.Compilation
{
    internal class BufferWriter
    {
        public Action<RelationBuffer> Initializer { get; set; }
        public Action<RelationBuffer>[] Queues { get; set; }
    }
}
