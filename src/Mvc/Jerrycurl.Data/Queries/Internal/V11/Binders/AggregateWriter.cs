using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class AggregateWriter
    {
        public int BufferIndex { get; set; }
        public DataReader Data { get; set; }
    }
}
