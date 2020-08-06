namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class AggregateBinder : ValueBinder
    {
        public int BufferIndex { get; set; }
        public bool IsPrincipal { get; set; }
    }
}
