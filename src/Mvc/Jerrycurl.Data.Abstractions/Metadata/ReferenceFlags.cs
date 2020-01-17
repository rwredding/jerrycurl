using System;

namespace Jerrycurl.Data.Metadata
{
    [Flags]
    public enum ReferenceFlags
    {
        None = 0,
        Parent = 1,
        Child = 2,
        Primary = 4,
        Foreign = 8,
        One = 16,
        Many = 32,
        Self = 64,
    }
}
