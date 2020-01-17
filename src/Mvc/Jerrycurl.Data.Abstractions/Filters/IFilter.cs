using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public interface IFilter
    {
        IFilterHandler GetHandler(IDbConnection connection);
        IFilterAsyncHandler GetAsyncHandler(IDbConnection connection);
    }
}
