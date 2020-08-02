using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class ValueKey
    {
        public IEnumerable<ValueBinder> Values { get; set; }
        public ParameterExpression Variable { get; set; }
    }
}
