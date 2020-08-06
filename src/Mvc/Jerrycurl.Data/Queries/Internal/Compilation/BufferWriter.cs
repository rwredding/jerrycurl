using System;
using System.Data;

namespace Jerrycurl.Data.Queries.Internal.Compilation
{
    internal class BufferWriter
    {
        public Action<IQueryBuffer> Initialize { get; set; }
        public Action<IQueryBuffer, IDataReader> WriteAll { get; set; }
        public Action<IQueryBuffer, IDataReader> WriteOne { get; set; }
    }
}
