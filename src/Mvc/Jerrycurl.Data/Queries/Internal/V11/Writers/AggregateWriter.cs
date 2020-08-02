using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.V11.Binders;

namespace Jerrycurl.Data.Queries.Internal.V11.Writers
{
    internal class AggregateWriter
    {
        public int BufferIndex { get; set; }
        public DataBinder Data { get; set; }
    }
}
