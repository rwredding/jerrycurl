using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11.Compilation
{
    internal delegate void ObjectBinder<TParent, TValue>(TParent parent, int index, TValue value);
}
