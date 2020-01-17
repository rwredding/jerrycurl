using System;

namespace Jerrycurl.Data.Metadata
{
    [Flags]
    public enum TableMetadataFlags
    {
        None = 0,
        Table = 1,
        Column = 2,
    }
}
