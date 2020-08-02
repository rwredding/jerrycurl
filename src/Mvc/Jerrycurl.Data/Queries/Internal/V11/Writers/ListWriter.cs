using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binders;

namespace Jerrycurl.Data.Queries.Internal.V11.Writers
{
    internal class ListWriter
    {
        public ParameterExpression Slot { get; set; }
        public int BufferIndex { get; set; }
        public NodeBinder Item { get; set; }
        public ValueKey PrimaryKey { get; set; }
        public ValueKey JoinKey { get; set; }
        public IBindingMetadata Metadata { get; set; }
    }
}
