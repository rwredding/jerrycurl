using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;
using System;
using System.Linq.Expressions;
using Jerrycurl.Relations;
using Jerrycurl.Vendors.SqlServer;
using System.Linq;

namespace Jerrycurl.Mvc.Sql.SqlServer
{
    public static class TvpExtensions
    {
        /// <summary>
        /// Appends a table-valued parameter from the current values, e.g. <c>@TP0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute TvpName(this IProjection projection)
        {
            if (projection.Source == null)
                throw ProjectionException.ValueNotFound(projection);
            else if (!projection.Attributes.Any())
                throw ProjectionException.FromProjection(projection, "No attributes found.");

            Relation relation = new Relation(projection.Source, projection.Attributes.Select(a => a.Metadata.Identity));

            string paramName = projection.Context.Lookup.Custom("TP", projection.Identity, metadata: projection.Metadata.Identity, field: projection.Source);
            string dialectName = projection.Context.Domain.Dialect.Parameter(paramName);

            return projection.Attr().Append(dialectName).Append(new TableValuedParameter(paramName, relation));
        }

        /// <summary>
        /// Appends a correlated table-valued parameter from the selected values, e.g. <c>@TP0 T0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Tvp<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Tvp();

        /// <summary>
        /// Appends a correlated table-valued parameter from the current values, e.g. <c>@TP0 T0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Tvp(this IProjection projection) => projection.TvpName().Append(" ").Ali();
    }
}
