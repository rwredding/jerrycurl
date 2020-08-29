using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11.Compilation
{
    internal delegate void FieldBinder<TParent, TValue>(TParent parent, int index, TValue value);
}
