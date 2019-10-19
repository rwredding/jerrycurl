using System;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Sql
{
    public static class StarExtensions
    {
        /// <summary>
        /// Appends mappings between the current qualified columns and their properties, e.g. <c>T0."MyColumn" AS "Item.MyValue"</c>, to the projection buffer. Identical to calling <c>Cols().As().Props()</c>.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Star(this IProjection projection)
        {
            if (!projection.Any())
                throw ProjectionException.FromProjection(projection, "No attributes found.");

            IProjectionMetadata metadata = ProjectionHelper.GetPreferredTableMetadata(projection).Identity.GetMetadata<IProjectionMetadata>();

            return projection.With(metadata).Cols().As().Props();
        }

        /// <summary>
        /// Appends mappings between the selected qualified columns and their properties, e.g. <c>T0."MyColumn" AS "Item.MyValue"</c>, to the projection buffer. Identical to calling <c>Cols().As().Props()</c>.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Star<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Star();
    }
}
