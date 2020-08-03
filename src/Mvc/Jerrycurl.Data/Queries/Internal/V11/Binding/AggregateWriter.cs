using System.Diagnostics;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    [DebuggerDisplay("{Data,nq}")]
    internal class AggregateWriter
    {
        public int BufferIndex { get; set; }
        public DataBinder Data { get; set; }
    }
}
