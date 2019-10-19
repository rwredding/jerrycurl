using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
