using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc
{
    public interface IProcRenderer
    {
        ISqlContent Body();
        ISqlContent Partial(string procName, IProjection model, IProjection result);
    }
}
