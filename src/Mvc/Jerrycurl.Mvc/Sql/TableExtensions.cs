using System;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Sql
{
    public static class TableExtensions
    {
        /// <summary>
        /// Appends the current table name, e.g. <c>"MyTable"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute TblName(this IProjectionAttribute attribute)
        {
            ITableMetadata table = ProjectionHelper.GetPreferredTableMetadata(attribute);

            string qualifier = attribute.Context.Domain.Dialect.Qualifier;
            string tableName = string.Join(qualifier, table.TableName.Select(attribute.Context.Domain.Dialect.Identifier));

            return attribute.Append(tableName);
        }

        /// <summary>
        /// Appends the current table alias, e.g. <c>T0</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Ali(this IProjectionAttribute attribute)
        {
            ITableMetadata table = ProjectionHelper.GetPreferredTableMetadata(attribute);

            string alias = attribute.Context.Lookup.Table(attribute.Identity, table.Identity);

            return attribute.Append(alias);
        }

        /// <summary>
        /// Appends the current correlated table name, e.g. <c>"MyTable" T0</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <param name="tblAlias">An alternative alias to use for the current table.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Tbl(this IProjectionAttribute attribute, string tblAlias = null)
        {
            attribute = attribute.TblName().Append(" ");

            if (tblAlias != null)
                return attribute.Append(attribute.Context.Domain.Dialect.Identifier(tblAlias));
            else
                return attribute.Ali();
        }

        /// <summary>
        /// Appends the current correlated table name, e.g. <c>"MyTable" T0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="tblAlias">An alternative alias to use for the current table.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Tbl(this IProjection projection, string tblAlias = null) => projection.Attr().Tbl(tblAlias);

        /// <summary>
        /// Appends the selected correlated table name, e.g. <c>"MyTable" T0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <param name="tblAlias">An alternative alias to use for the selected table.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Tbl<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string tblAlias = null) => projection.For(expression).Tbl(tblAlias);

        /// <summary>
        /// Appends the current table name, e.g. <c>"MyTable"</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute TblName(this IProjection projection) => projection.Attr().TblName();

        /// <summary>
        /// Appends the selected table name, e.g. <c>"MyTable"</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute TblName<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).TblName();


        /// <summary>
        /// Appends the current table alias, e.g. <c>T0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Ali(this IProjection projection) => projection.Attr().Ali();

        /// <summary>
        /// Appends the selected table alias, e.g. <c>T0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Ali<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).Ali();

    }
}
