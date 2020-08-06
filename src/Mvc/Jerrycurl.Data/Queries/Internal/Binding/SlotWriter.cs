using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class SlotWriter
    {
        public int BufferIndex { get; set; }
        public Type KeyType { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public ParameterExpression Variable { get; set; }
    }
}
