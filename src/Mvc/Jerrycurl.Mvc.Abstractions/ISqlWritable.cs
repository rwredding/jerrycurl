namespace Jerrycurl.Mvc
{
    public interface ISqlWritable
    {
        void WriteTo(ISqlBuffer buffer);
    }
}
