using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11
{
    public interface IFieldData
    {
        public object Relation { get; }
        public int Index { get; }
        public object Parent { get; }
        public object Value { get; }
    }
}
