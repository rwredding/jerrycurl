using System;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    [Flags]
    internal enum NodeFlags
    {
        None = 0,
        Key = 1,
        Dynamic = 2,
        Result = 4,
    }
}
