using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class KeyBinder
    {
        public IEnumerable<ValueBinder> Values { get; set; }
        public ParameterExpression Variable { get; set; }
        public ParameterExpression Slot { get; set; }
        public ParameterExpression Array { get; set; }
        public int BufferIndex { get; set; }
        public IReference Metadata { get; set; }
        public Type KeyType { get; set; }
    }
}
