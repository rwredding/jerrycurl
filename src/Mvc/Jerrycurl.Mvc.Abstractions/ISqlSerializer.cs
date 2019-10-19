using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface ISqlSerializer<TData>
    {
        IEnumerable<TData> Serialize(ISqlOptions options);
    }
}
