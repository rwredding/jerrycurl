using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class AggregateBinder : ValueBinder
    {
        public int BufferIndex { get; set; }
    }
}
