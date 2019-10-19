using System;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Sql
{
    public static class ColumnExtensions
    {
        /// <summary>
        /// Appends the current column name in unqualified form, e.g. <c>"MyColumn"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute ColName(this IProjectionAttribute attribute)
        {
            ITableMetadata column = ProjectionHelper.GetPreferredColumnMetadata(attribute);

            string identifier = attribute.Context.Domain.Dialect.Identifier(column.ColumnName);

            return attribute.Append(identifier);
        }

        /// <summary>
        /// Appends the current column name in qualified form, e.g. <c>T0."MyColumn"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <param name="tblAlias">The table alias to qualify the column name with.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Col(this IProjectionAttribute attribute, string tblAlias = null)
        {
            if (tblAlias != null)
                attribute = attribute.Append(attribute.Context.Domain.Dialect.Identifier(tblAlias));
            else
                attribute = attribute.Ali();

            return attribute.Append(attribute.Context.Domain.Dialect.Qualifier).ColName();
        }

        /// <summary>
        /// Appends the selected column names in qualified form, e.g. <c>T0."MyColumn"</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="tblAlias">The table alias to qualify each column name with.</param>
        /// <overloads></overloads>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Cols(this IProjection projection, string tblAlias = null) => projection.Map(a => a.Col(tblAlias));

        /// <summary>
        /// Navigates to the selected projection and appends the selected column names in qualified form, e.g. <c>T0."MyColumn"</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <param name="tblAlias">The table alias to qualify each column name with.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Cols<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string tblAlias = null) => projection.For(expression).Cols(tblAlias);

        /// <summary>
        /// Appends the selected column names in unqualified form, e.g. <c>T0."MyColumn"</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection ColNames(this IProjection projection) => projection.Map(a => a.ColName());

        /// <summary>
        /// Navigates to the selected projection and appends the selected column names in unqualified form, e.g. <c>"MyColumn"</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection ColNames<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).ColNames();

        /// <summary>
        /// Appends the current column name in qualified form, e.g. <c>T0."MyColumn"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="tblAlias">The table alias to qualify the column name with.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Col(this IProjection projection, string tblAlias = null) => projection.Attr().Col(tblAlias);

        /// <summary>
        /// Navigates to the selected attribute and appends the current column name in qualified form, e.g. <c>T0."MyColumn"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <param name="tblAlias">The table alias to qualify the column name with.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Col<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string tblAlias = null) => projection.Attr(expression).Col(tblAlias);

        /// <summary>
        /// Appends the current column name in unqualified form, e.g. <c>"MyColumn"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute ColName(this IProjection projection) => projection.Attr().ColName();

        /// <summary>
        /// Navigates to the selected attribute and appends the current column name in unqualified form, e.g. <c>"MyColumn"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute ColName<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.Attr(expression).ColName();

    }
}