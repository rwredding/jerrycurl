namespace Jerrycurl.Mvc
{
    public class PageExecutionContext : IPageExecutionContext
    {
        public PageDescriptor Page { get; set; }
        public BodyFactory Body { get; set; }
        public ISqlBuffer Buffer { get; set; }
    }
}
