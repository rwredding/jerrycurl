using System;
using System.Linq.Expressions;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    internal class ValueBinder : NodeBinder
    {
        public bool CanBeDbNull { get; set; }
        public ParameterExpression IsDbNull { get; set; }
        public ParameterExpression Variable { get; set; }
        public Type KeyType { get; set; }
    }
}
