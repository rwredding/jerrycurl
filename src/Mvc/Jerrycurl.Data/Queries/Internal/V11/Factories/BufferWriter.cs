using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Factories
{
    internal class BufferWriter
    {
        public Action<IQueryBuffer> Initialize { get; set; }
        public Action<IQueryBuffer, IDataReader> WriteAll { get; set; }
        public Action<IQueryBuffer, IDataReader> WriteOne { get; set; }
    }
}
