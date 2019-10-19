using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
