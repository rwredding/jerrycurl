using System;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Vendors.Oracle;

namespace Jerrycurl.Mvc.Sql.Oracle
{
    public static class RefcursorExtensions
    {
        public static IProjectionAttribute Refcursor(this IProjectionAttribute attribute)
        {
            string paramName = attribute.Context.Lookup.Custom("R");
            string dialectName = attribute.Context.Domain.Dialect.Parameter(paramName);

            Refcursor param = new Refcursor(paramName);

            return attribute.Append(dialectName).Append(param);
        }

        public static IProjectionAttribute Refcursor<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Refcursor();
        public static IProjectionAttribute Refcursor(this IProjection projection) => projection.Attr().Refcursor();
    }
}