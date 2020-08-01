using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class AggregateReader : NodeReader
    {
        public int BufferIndex { get; set; }
        public ParameterExpression IsDbNull { get; set; }
        public ParameterExpression Value { get; set; }
        public bool CanBeDbNull { get; set; }
    }
}
