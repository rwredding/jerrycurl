using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Mvc;
using Jerrycurl.Vendors.Oracle;
using Jerrycurl.Data.Sessions;

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