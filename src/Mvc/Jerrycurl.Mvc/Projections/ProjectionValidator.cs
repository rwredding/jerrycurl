using System.Linq;

namespace Jerrycurl.Mvc.Projections
{
    internal static class ProjectionValidator
    {
        public static void ValidateMetadata(IProjection projection)
        {
            if (!projection.Identity.Schema.Equals(projection.Metadata.Identity.Schema))
                throw ProjectionException.FromProjection(projection, "Metadata and identity must belong to the same schema.");
        }

        public static void ValidateField(IProjection projection)
        {
            if (projection.Source != null && !projection.Identity.Schema.Equals(projection.Source.Identity.Schema))
                throw ProjectionException.FromProjection(projection, "Field and identity must belong to the same schema.");
        }

        public static void ValidateAttributes(IProjection projection)
        {
            if (projection.Attributes.Any(a => !a.Metadata.Identity.Schema.Equals(projection.Metadata.Identity.Schema)))
                throw ProjectionException.FromProjection(projection, "All attributes must belong to the same projection schema.");
        }

        public static void ValidateIdentity(IProjectionIdentity identity)
        {
            if (identity.Field != null && !identity.Schema.Equals(identity.Field.Identity.Schema))
                throw new ProjectionException("Unable to create projection identity. Field schema mismatch.");

        }
    }
}
