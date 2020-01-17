namespace Jerrycurl.Mvc
{
    public interface IPageExecutionContext
    {
        PageDescriptor Page { get; }
        ISqlBuffer Buffer { get; }
        BodyFactory Body { get; }
    }
}
