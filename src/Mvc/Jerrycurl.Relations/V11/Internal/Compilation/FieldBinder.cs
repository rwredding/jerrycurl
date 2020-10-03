using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11.Internal.Compilation
{
    internal delegate void FieldBinder<TParent, TValue>(TParent parent, int index, TValue value);
}
