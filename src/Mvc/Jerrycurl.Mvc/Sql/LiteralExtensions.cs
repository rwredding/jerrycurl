using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.Sql
{
    public static class LiteralExtensions
    {
        /// <summary>
        /// Appends the current value in safe literal form, e.g. <c>1</c>, to the attribute buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Lit(this IProjectionAttribute attribute)
        {
            IField field = ProjectionHelper.GetFieldValue(attribute);

            string literal = attribute.Context.Domain.Dialect.Literal(field?.Value);

            if (literal == null)
                return attribute.Par();
            else
                return attribute.Append(literal);
        }

        /// <summary>
        /// Appends the current value in safe literal form, e.g. <c>1</c>, to a new attribute buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="projection">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Lit(this IProjection projection) => projection.Attr().Lit();

        /// <summary>
        /// Appends the selected value in safe literal form, e.g. <c>1</c>, to a new attribute buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="projection">The current attribute.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Lit<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).Lit();

        /// <summary>
        /// Appends the current values in safe literal form, e.g. <c>1</c>, to the projection buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Lits(this IProjection projection) => projection.Map(a => a.Lit());

        /// <summary>
        /// Appends the selected values in safe literal form, e.g. <c>1</c>, to the projection buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Lits<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Lits();

        /// <summary>
        /// Appends a comma-separated list of safe literals, e.g. <c>1, 2, 3</c>, to a new attribute buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute LitList(this IProjection projection) => projection.ValList(a => a.Lit());

        /// <summary>
        /// Appends a comma-separated list of safe literals, e.g. <c>1, 2, 3</c>, to a new attribute buffer. Parameters are used for unsafe literals.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute LitList<TModel, TItem>(this IProjection<TModel> projection, Expression<Func<TModel, IEnumerable<TItem>>> expression)
            => projection.Open(expression).LitList();
    }
}
