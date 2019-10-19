using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jerrycurl.Mvc.Sql.SqlServer
{
    internal static class ProjectionHelper
    {
        public static IJsonMetadata GetJsonMetadata(IProjectionAttribute attribute) => attribute.Metadata.Identity.GetMetadata<IJsonMetadata>() ??
            throw ProjectionException.FromProjection(attribute, "JSON metadata not found.");
    }
}
