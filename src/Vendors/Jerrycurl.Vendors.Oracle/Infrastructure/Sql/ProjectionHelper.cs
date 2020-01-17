using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Sql.Oracle
{
    internal static class ProjectionHelper
    {
        public static IJsonMetadata GetJsonMetadata(IProjectionAttribute attribute) => attribute.Metadata.Identity.GetMetadata<IJsonMetadata>() ??
            throw ProjectionException.FromProjection(attribute, "JSON metadata not found.");
    }
}
