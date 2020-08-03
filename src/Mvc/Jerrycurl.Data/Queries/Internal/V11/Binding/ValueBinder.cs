using System.Linq.Expressions;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    internal class ValueBinder : NodeBinder
    {
        public bool CanBeDbNull { get; set; }
        public ParameterExpression IsDbNull { get; set; }
        public ParameterExpression Value { get; set; }
    }
}
