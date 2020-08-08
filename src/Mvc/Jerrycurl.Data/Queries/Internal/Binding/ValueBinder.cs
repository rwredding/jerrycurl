using System;
using System.Linq.Expressions;
using Jerrycurl.Data.Queries.Internal.Parsing;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class ValueBinder : NodeBinder
    {
        public ValueBinder(Node node)
            : base(node)
        {

        }
        public bool CanBeDbNull { get; set; }
        public ParameterExpression IsDbNull { get; set; }
        public ParameterExpression Variable { get; set; }
        public Type KeyType { get; set; }
    }
}
