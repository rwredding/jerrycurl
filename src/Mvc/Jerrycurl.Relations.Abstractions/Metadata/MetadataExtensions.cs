namespace Jerrycurl.Relations.Metadata
{
    public static class MetadataExtensions
    {
        public static bool HasFlag(this IRelationMetadata metadata, RelationMetadataFlags flag) => (metadata.Flags & flag) == flag;
        public static bool HasAnyFlag(this IRelationMetadata metadata, RelationMetadataFlags flag) => (metadata.Flags & flag) != RelationMetadataFlags.None;
    }
}
