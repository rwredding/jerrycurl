using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Nodes
{
    [Flags]
    internal enum NodeFlags
    {
        None = 0,
        Container = 1,
        Value = 2,
        Dynamic = 4,
    }
}
