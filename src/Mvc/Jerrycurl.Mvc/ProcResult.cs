namespace Jerrycurl.Mvc
{
    internal class ProcResult : IProcResult
    {
        public ISqlBuffer Buffer { get; set; }
        public IDomainOptions Domain { get; set; }
    }
}
