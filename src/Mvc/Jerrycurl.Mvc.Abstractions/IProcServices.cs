using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc
{
    public interface IProcServices
    {
        TService GetService<TService>() where TService : class;
        IProjection<TModel> GetProjection<TModel>(IProcContext context);
    }
}
