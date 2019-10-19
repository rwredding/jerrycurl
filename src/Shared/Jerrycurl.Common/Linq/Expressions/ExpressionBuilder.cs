using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jerrycurl.Linq.Expressions
{
    internal abstract class ExpressionBuilder<TState>
        where TState : new()
    {
        private readonly Dictionary<object, TState> stateMap = new Dictionary<object, TState>();
        private readonly Dictionary<object, List<Expression>> exprMap = new Dictionary<object, List<Expression>>();
        private readonly Dictionary<Expression, object> idMap = new Dictionary<Expression, object>();
        private readonly List<ParameterExpression> variables = new List<ParameterExpression>();

        public Expression Add(Expression expression, params object[] identity)
        {
            CompositeKey key = new CompositeKey(identity);

            if (this.exprMap.Any(p => !p.Key.Equals(key) && p.Value.Contains(expression)))
                throw new InvalidOperationException("An identical Expression reference with different key found.");

            if (!this.exprMap.TryGetValue(key, out List<Expression> exprs))
                this.exprMap[key] = exprs = new List<Expression>();

            exprs.Add(expression);

            this.idMap[expression] = key;
            this.stateMap[key] = new TState();

            return expression;
        }

        public Expression Build(Expression expression)
        {
            Visitor scanner = new Visitor(this.ScanHandler);
            Visitor builder = new Visitor(this.BuildHandler);

            scanner.Visit(expression);

            Expression built = builder.Visit(expression);

            if (this.variables.Count > 0)
                return Expression.Block(this.variables, built);

            return built;
        }

        public BlockExpression BuildBlock(Expression body, IEnumerable<ParameterExpression> variables)
        {
            foreach (ParameterExpression variable in variables)
                this.variables.Add(variable);

            Expression built = this.Build(body);

            switch (built)
            {
                case BlockExpression block:
                    return block;
                default:
                    return Expression.Block(built);
            }
        }

        protected abstract void OnScan(ExpressionBuilderContext<TState> context);
        protected abstract Expression OnBuild(ExpressionBuilderContext<TState> context);

        private Expression ScanHandler(Expression expression)
        {
            if (this.idMap.TryGetValue(expression, out object key))
            {
                ExpressionBuilderContext<TState> context = new ExpressionBuilderContext<TState>()
                {
                    Key = key,
                    Batch = this.exprMap[key],
                    Expression = expression,
                    State = this.stateMap[key],
                    Variables = new List<ParameterExpression>(),
                };

                this.OnScan(context);
            }


            return expression;
        }

        private Expression BuildHandler(Expression expression)
        {
            if (this.idMap.TryGetValue(expression, out object key))
            {
                ExpressionBuilderContext<TState> context = new ExpressionBuilderContext<TState>()
                {
                    Key = key,
                    Batch = this.exprMap[key],
                    Expression = expression,
                    State = this.stateMap[key],
                    Variables = new List<ParameterExpression>(),
                };

                Expression result = this.OnBuild(context);

                this.variables.AddRange(context.Variables);

                return result;
            }

            return expression;
        }

        private class Visitor : ExpressionVisitor
        {
            private readonly Func<Expression, Expression> handler;

            public Visitor(Func<Expression, Expression> handler)
            {
                this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            protected override Expression VisitIndex(IndexExpression node)
            {
                return this.handler(base.VisitIndex(node));
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                return this.handler(base.VisitBinary(node));
            }

            protected override Expression VisitBlock(BlockExpression node)
            {
                return this.handler(base.VisitBlock(node));
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                return this.handler(base.VisitConditional(node));
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return this.handler(base.VisitConstant(node));
            }

            protected override Expression VisitDebugInfo(DebugInfoExpression node)
            {
                return this.handler(base.VisitDebugInfo(node));
            }

            protected override Expression VisitDefault(DefaultExpression node)
            {
                return this.handler(base.VisitDefault(node));
            }

            protected override Expression VisitExtension(Expression node)
            {
                return this.handler(base.VisitExtension(node));
            }

            protected override Expression VisitGoto(GotoExpression node)
            {
                return this.handler(base.VisitGoto(node));
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                return this.handler(base.VisitInvocation(node));
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return this.handler(base.VisitLambda(node));
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                return this.handler(base.VisitMemberInit(node));
            }

            protected override Expression VisitLabel(LabelExpression node)
            {
                return this.handler(base.VisitLabel(node));
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                return this.handler(base.VisitMember(node));
            }

            protected override Expression VisitListInit(ListInitExpression node)
            {
                return this.handler(base.VisitListInit(node));
            }

            protected override Expression VisitLoop(LoopExpression node)
            {
                return this.handler(base.VisitLoop(node));
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return this.handler(base.VisitMethodCall(node));
            }

            protected override Expression VisitNew(NewExpression node)
            {
                return this.handler(base.VisitNew(node));
            }

            protected override Expression VisitNewArray(NewArrayExpression node)
            {
                return this.handler(base.VisitNewArray(node));
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                return this.handler(base.VisitUnary(node));
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return this.handler(base.VisitParameter(node));
            }

            protected override Expression VisitTry(TryExpression node)
            {
                return this.handler(base.VisitTry(node));
            }

            protected override Expression VisitTypeBinary(TypeBinaryExpression node)
            {
                return this.handler(base.VisitTypeBinary(node));
            }

            protected override Expression VisitSwitch(SwitchExpression node)
            {
                return this.handler(base.VisitSwitch(node));
            }

            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
            {
                return this.handler(base.VisitRuntimeVariables(node));
            }
        }
    }
}
