namespace Jerrycurl.Mvc
{
    internal class ProcContext : IProcContext
    {
        public IDomainOptions Domain { get; }
        public IProcLocator Locator { get; }
        public IProcRenderer Renderer { get; }
        public IProcLookup Lookup { get; }
        public IPageExecutionContext Executing => this.Stack.Current;
        public ProcExecutionStack Stack { get; }

        public ProcContext(PageDescriptor descriptor, IDomainOptions domain)
        {
            this.Locator = descriptor.Locator;
            this.Domain = domain;
            this.Renderer = new ProcRenderer(this);
            this.Lookup = new ProcLookup();
            this.Stack = new ProcExecutionStack();
        }
    }
}
