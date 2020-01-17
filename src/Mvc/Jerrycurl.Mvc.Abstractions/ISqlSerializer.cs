using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public interface ISqlSerializer<TData>
    {
        IEnumerable<TData> Serialize(ISqlOptions options);
    }
}
