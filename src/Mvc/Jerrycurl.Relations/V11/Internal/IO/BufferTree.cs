using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11.Internal.IO
{
    internal class BufferTree
    {
        public DotNotation2 Notation { get; set; }
        public SourceReader Source { get; set; }
        public List<QueueReader> Queues { get; } = new List<QueueReader>();
        public List<FieldWriter> Fields { get; } = new List<FieldWriter>();
    }
}
