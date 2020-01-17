using System;

namespace Jerrycurl.Relations.Internal.Nodes
{
    [Flags]
    internal enum NodeFlags
    {
        None = 0,
        Source = 1,
        List = 2,
        Item = 4,
        Field = 8,
        Product = 16,
    }
}
