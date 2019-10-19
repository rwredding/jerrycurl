using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jerrycurl.Linq.Expressions
{
    internal class ExpressionBuilderContext<TState>
        where TState : new()
    {
        public object Key { get; internal set; }
        public Expression Expression { get; internal set; }
        public IEnumerable<Expression> Batch { get; internal set; }
        public IList<ParameterExpression> Variables { get; internal set; }
        public TState State { get; internal set; }
    }
}
