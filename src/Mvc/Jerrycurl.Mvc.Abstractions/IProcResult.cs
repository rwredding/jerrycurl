namespace Jerrycurl.Mvc
{
    public interface IProcResult
    {
        ISqlBuffer Buffer { get; }
        IDomainOptions Domain { get; }
    }
}
