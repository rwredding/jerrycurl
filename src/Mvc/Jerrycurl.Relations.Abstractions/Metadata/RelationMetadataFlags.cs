using System;

namespace Jerrycurl.Relations.Metadata
{
    [Flags]
    public enum RelationMetadataFlags
    {
        None = 0,
        Model = 1,
        List = 2,
        Item = 4,
        Property = 8,
        Readable = 16,
        Writable = 32,
        Recursive = 64,
    }
}
