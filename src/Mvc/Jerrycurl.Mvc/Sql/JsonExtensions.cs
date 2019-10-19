using Jerrycurl.Mvc.Projections;
using System;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Metadata;

namespace Jerrycurl.Mvc.Sql
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Appends the current JSON path literal, e.g. <c>'$.my.value'</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute JsonPath(this IProjectionAttribute attribute)
        {
            IJsonMetadata metadata = attribute.Metadata.Identity.GetMetadata<IJsonMetadata>();

            if (metadata == null)
                throw ProjectionException.FromProjection(attribute, "JSON metadata not found.");

            string literal = attribute.Context.Domain.Dialect.String(metadata.Path);

            return attribute.Append(literal);
        }

        /// <summary>
        /// Appends the current JSON path literal, e.g. <c>'$.my.value'</c>, to the attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute JsonPath(this IProjection projection) => projection.Attr().JsonPath();

        /// <summary>
        /// Appends the selected JSON path literal, e.g. <c>'$.my.value'</c>, to the attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute JsonPath<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).JsonPath();
    }
}
