using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;

namespace Jerrycurl.Mvc
{
    public interface IProcBuffer : ISqlBuffer, ISqlSerializer<QueryData>, ISqlSerializer<CommandData>
    {

    }
}
