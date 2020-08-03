using System;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    [Flags]
    internal enum NodeFlags
    {
        None = 0,
        Dynamic = 1,
        Result = 2,
        Item = 4,
        Aggregate = 8,
    }
}
