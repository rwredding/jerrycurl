using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IProcBuffer : ISqlBuffer, ISqlSerializer<QueryData>, ISqlSerializer<CommandData>
    {

    }
}
