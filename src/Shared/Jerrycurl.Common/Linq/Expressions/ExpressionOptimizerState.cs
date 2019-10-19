using System.Linq.Expressions;

namespace Jerrycurl.Linq.Expressions
{
    internal class ExpressionOptimizerState
    {
        public int Hits { get; set; }
        public int Index { get; set; }
        public ParameterExpression Variable { get; set; }
    }
}
