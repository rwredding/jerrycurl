using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface IFieldData
    {
        public object Parent { get; }
        public int Index { get; }
        public object Value { get; }
    }
}
