using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jerrycurl.Relations.Metadata
{
    public class DotNotation : IMetadataNotation
    {
        public virtual IEqualityComparer<string> Comparer { get; }

        public DotNotation()
            : this(StringComparer.OrdinalIgnoreCase)
        {

        }

        public DotNotation(IEqualityComparer<string> comparer)
        {
            this.Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public string Combine(params string[] parts)
        {
            return string.Join(".", parts.Where(p => p.Length > 0));
        }

        public string Combine(string part1, string part2)
        {
            if (part1 == "")
                return part2;
            else if (part2 == "")
                return part1;

            return part1 + "." + part2;
        }

        public string Model() => "";
        public string Index(string name, int index) => $"{name}[{index}]";
        public bool Equals(string name1, string name2) => this.Comparer.Equals(name1, name2);

        public string Lambda(LambdaExpression expression)
        {
            Stack<string> stack = new Stack<string>();

            Expression expr = expression.Body;

            while (expr != null)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            MemberExpression memberExpr = (MemberExpression)expr;

                            stack.Push(memberExpr.Member.Name);

                            expr = memberExpr.Expression;
                        }
                        break;
                    case ExpressionType.Parameter:
                        return this.Combine(stack.ToArray());
                    default:
                        expr = null;
                        break;
                }
            }

            return null;
        }

        public string Path(string from, string to)
        {
            if (this.Comparer.Equals(from, to))
                return this.Model();
            else if (this.Comparer.Equals(from, this.Model()))
                return to;

            if (to.Length <= from.Length + 2 || !this.Comparer.Equals(from, to.Substring(0, from.Length)) || to[from.Length] != '.')
                throw new InvalidOperationException();

            return to.Remove(0, from.Length + 1);
        }

        public string Parent(string name)
        {
            if (this.Comparer.Equals(name, this.Model()))
                return null;

            string[] parts = name.Split(new[] { '.' });

            return this.Combine(parts.Take(parts.Length - 1).ToArray());
        }

        public string Member(string name)
        {
            string[] parts = name.Split(new[] { '.' });

            return parts.Last();
        }
    }
}
