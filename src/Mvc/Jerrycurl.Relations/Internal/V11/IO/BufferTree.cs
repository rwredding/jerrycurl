using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class BufferTree
    {
        public List<SetReader> Sets { get; } = new List<SetReader>();
        public List<FieldWriter> Fields { get; } = new List<FieldWriter>();
    }
}
