using System.Linq.Expressions;

namespace Jerrycurl.Linq.Expressions
{
    internal class ExpressionOptimizer : ExpressionBuilder<ExpressionOptimizerState>
    {
        protected override void OnScan(ExpressionBuilderContext<ExpressionOptimizerState> context)
        {
            context.State.Hits++;
        }

        protected override Expression OnBuild(ExpressionBuilderContext<ExpressionOptimizerState> context)
        {
            if (context.State.Hits <= 1)
                return context.Expression;

            if (context.State.Index++ == 0)
            {
                ParameterExpression variable = context.State.Variable = Expression.Variable(context.Expression.Type);

                context.Variables.Add(variable);

                return Expression.Assign(variable, context.Expression);
            }

            return context.State.Variable;
        }
    }
}