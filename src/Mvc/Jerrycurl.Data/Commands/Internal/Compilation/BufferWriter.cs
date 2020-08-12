using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Commands.Internal.Compilation
{
    internal delegate void BufferWriter(IDataReader dataReader, FieldBuffer[] buffers);
}
