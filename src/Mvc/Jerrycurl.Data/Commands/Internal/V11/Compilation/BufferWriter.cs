using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Commands.Internal.V11.Compilation
{
    internal delegate void BufferWriter(IDataReader dataReader, FieldPipe[] pipes);
}
