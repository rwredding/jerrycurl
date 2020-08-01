using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class ListWriter
    {
        public ParameterExpression Slot { get; set; }
        public int BufferIndex { get; set; }
        public NodeReader Item { get; set; }
        public KeyReader PrimaryKey { get; set; }
        public KeyReader JoinKey { get; set; }
        public IBindingMetadata Metadata { get; set; }
    }
}
