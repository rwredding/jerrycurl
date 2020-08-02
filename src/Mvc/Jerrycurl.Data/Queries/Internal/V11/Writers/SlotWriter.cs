using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Writers
{
    internal class SlotWriter
    {
        public int BufferIndex { get; set; }
        public IList<Type> Key { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public ParameterExpression Variable { get; set; }
    }
}
