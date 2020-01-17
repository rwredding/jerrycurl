using Jerrycurl.Relations.Metadata;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Mvc.Metadata
{
    public static class MetadataExtensions
    {
        public static bool HasFlag(this IProjectionMetadata metadata, TableMetadataFlags flag) => (metadata.Table != null && metadata.Table.HasFlag(flag));
        public static bool HasAnyFlag(this IProjectionMetadata metadata, TableMetadataFlags flag) => (metadata.Table != null && metadata.Table.HasAnyFlag(flag));

        public static bool HasFlag(this IProjectionMetadata metadata, RelationMetadataFlags flag) => (metadata.Relation != null && metadata.Relation.HasFlag(flag));
        public static bool HasAnyFlag(this IProjectionMetadata metadata, RelationMetadataFlags flag) => (metadata.Relation != null && metadata.Relation.HasAnyFlag(flag));

        public static bool HasFlag(this IProjectionMetadata metadata, ReferenceMetadataFlags flag) => (metadata.Relation != null && metadata.Reference.HasFlag(flag));
        public static bool HasAnyFlag(this IProjectionMetadata metadata, ReferenceMetadataFlags flag) => (metadata.Relation != null && metadata.Reference.HasAnyFlag(flag));

        public static bool HasFlag(this IProjectionMetadata metadata, ProjectionMetadataFlags flag) => (metadata.Flags & flag) == flag;
        public static bool HasAnyFlag(this IProjectionMetadata metadata, ProjectionMetadataFlags flag) => (metadata.Flags & flag) != ProjectionMetadataFlags.None;
    }
}
