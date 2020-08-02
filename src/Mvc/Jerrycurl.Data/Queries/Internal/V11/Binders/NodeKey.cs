using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class KeyReader
    {
        public IEnumerable<NodeReader> Values { get; set; }
        public ParameterExpression Variable { get; set; }
    }
}
