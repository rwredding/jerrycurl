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

namespace Jerrycurl.Mvc.Sql.Oracle
{
    public static class RefcursorExtensions
    {
        public static IProjectionAttribute Refcursor(this IProjectionAttribute attribute)
        {
            string paramName = attribute.Context.Lookup.Custom("R", attribute.Identity, attribute.Metadata.Identity, attribute.Field?.Invoke());
            string dialectName = attribute.Context.Domain.Dialect.Parameter(paramName);

            Parameter param = new Parameter(paramName, contract: new RefcursorContract());

            return attribute.Append(dialectName).Append(param);
        }

        public static IProjectionAttribute Refcursor<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Refcursor();
        public static IProjectionAttribute Refcursor(this IProjection projection) => projection.Attr().Refcursor();
    }
}