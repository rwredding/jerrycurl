using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public interface IFilter
    {
        IFilterHandler GetHandler();
    }
}
