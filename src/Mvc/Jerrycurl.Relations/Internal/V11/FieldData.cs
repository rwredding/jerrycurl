using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11
{
    public class FieldData<TValue, TParent>
    {
        public TParent Parent { get; }
        public int Index { get; }
        public TValue Value { get; }
    }
}
