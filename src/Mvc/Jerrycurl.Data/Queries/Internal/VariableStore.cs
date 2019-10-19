using Jerrycurl.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class VariableStore
    {
        private readonly Dictionary<(string, Type), ParameterExpression> cache = new Dictionary<(string, Type), ParameterExpression>();
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        public bool HasVariable(string name, Type variableType = null) => this.cache.ContainsKey((name, variableType));

        public ParameterExpression Add(string name, Type variableType)
        {
            ParameterExpression newVariable = Expression.Parameter(variableType, name);

            this.cache.Add((name, null), newVariable);

            return newVariable;
        }

        public ParameterExpression AddWithKey(string name, Type variableType)
        {
            int subIndex = this.counts.TryGetValue(name);

            ParameterExpression newVariable = Expression.Parameter(variableType, name + "_" + subIndex);

            this.cache.Add((name, variableType), newVariable);
            this.counts[name] = subIndex + 1;

            return newVariable;
        }

        public ParameterExpression Get(string name, Type variableType = null) => this.cache.TryGetValue((name, variableType));

        public IEnumerable<ParameterExpression> ToExpressions() => this.cache.Values;
        public Expression ToBlock(Expression body) => this.cache.Any() ? Expression.Block(this.ToExpressions(), body) : body;
        public Expression ToBlock(IEnumerable<Expression> body) => Expression.Block(this.ToExpressions(), body);
    }
}
