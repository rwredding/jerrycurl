
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11
{
    internal class RelationBuffer
    {
        public IField[] Fields { get; }
        public IEnumerator[] Enumerators { get; }
        public object[] Values { get; }
    }
}
