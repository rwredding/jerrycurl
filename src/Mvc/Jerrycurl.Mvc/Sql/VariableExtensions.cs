using System;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.Sql
{
    public static class VariableExtensions
    {
        /// <summary>
        /// Appends the current variable name, e.g. <c>@V0</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Var(this IProjectionAttribute attribute)
        {
            if (attribute.Field == null)
                throw ProjectionException.ValueNotFound(attribute);

            IField field = ProjectionHelper.GetFieldValue(attribute);

            string variableName = attribute.Context.Lookup.Variable(attribute.Identity, field);
            string dialectName = attribute.Context.Domain.Dialect.Variable(variableName);

            return attribute.Append(dialectName);
        }

        /// <summary>
        /// Appends the current variable names, e.g. <c>@V0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Vars(this IProjection projection) => projection.Map(a => a.Var());

        /// <summary>
        /// Appends the selected variable names, e.g. <c>@V0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Vars<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Vars();

        /// <summary>
        /// Appends the current variable name, e.g. <c>@V0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Var(this IProjection projection) => projection.Attr().Var();

        /// <summary>
        /// Appends the selected variable name, e.g. <c>@V0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Var<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).Var();
    }
}
