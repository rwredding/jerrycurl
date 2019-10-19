using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Metadata
{
    [Flags]
    public enum BindingMetadataFlags
    {
        None = 0,
        List = 1,
        Item = 2,
        Dynamic = 4,
        Model = 8,
        Readable = 16,
        Writable = 32,
    }
}
