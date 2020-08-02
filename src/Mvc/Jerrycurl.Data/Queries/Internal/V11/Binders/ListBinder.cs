using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class ListBinder : NodeBinder
    {
        public ParameterExpression Array { get; set; }
        public int BufferIndex { get; set; }
    }
}
