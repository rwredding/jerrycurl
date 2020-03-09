namespace Jerrycurl.Mvc
{
    public interface IProcContext
    {
        IProcLocator Locator { get; }
        IDomainOptions Domain { get; }
        IProcRenderer Renderer { get; }
        IProcLookup Lookup { get; }
        IPageExecutionContext Execution { get; }
    }
}
