using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc;

namespace Jerrycurl.Mvc.Sql.SqlServer
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Appends a call to <c>JSON_VALUE</c> from the current column and JSON path, e.g. <c>JSON_VALUE(T0."MyJson", '$.my.value')</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Json(this IProjectionAttribute attribute)
        {
            IJsonMetadata json = ProjectionHelper.GetJsonMetadata(attribute);

            IProjectionMetadata metadata = attribute.Metadata;
            IProjectionMetadata rootMetadata = json.MemberOf.Identity.GetMetadata<IProjectionMetadata>();

            attribute = attribute.Append("JSON_VALUE(");
            try
            {
                attribute = attribute.With(metadata: rootMetadata).Col();
            }
            catch (ProjectionException ex)
            {
                throw ProjectionException.FromProjection(attribute, "JSON value requires a column to read from.", innerException: ex);
            }
            attribute = attribute.Append(",");
            attribute = attribute.With(metadata: metadata).JsonPath();
            attribute = attribute.Append(")");

            return attribute;
        }

        /// <summary>
        /// Appends a call to <c>JSON_VALUE</c> from the current column and JSON path, e.g. <c>JSON_VALUE(T0."MyJson", '$.my.value')</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Json(this IProjection projection) => projection.Attr().Json();

        /// <summary>
        /// Appends a call to <c>JSON_VALUE</c> from the selected column and JSON path, e.g. <c>JSON_VALUE(T0."MyJson", '$.my.value')</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Json<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).Json();

    }
}
