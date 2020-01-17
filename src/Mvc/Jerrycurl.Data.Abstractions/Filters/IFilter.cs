using System.Data;

namespace Jerrycurl.Data.Filters
{
    public interface IFilter
    {
        IFilterHandler GetHandler(IDbConnection connection);
        IFilterAsyncHandler GetAsyncHandler(IDbConnection connection);
    }
}
