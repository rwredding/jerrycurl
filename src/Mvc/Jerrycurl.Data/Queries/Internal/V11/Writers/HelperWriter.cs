using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Writers
{
    internal class HelperWriter
    {
        public int BufferIndex { get; set; }
        public object Object { get; set; }
        public ParameterExpression Variable { get; set; }
    }
}
