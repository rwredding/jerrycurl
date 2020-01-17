using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public static class MetadataExtensions
    {
        public static bool HasFlag(this ITableMetadata metadata, TableMetadataFlags flag) => (metadata.Flags & flag) == flag;
        public static bool HasAnyFlag(this ITableMetadata metadata, TableMetadataFlags flag) => (metadata.Flags & flag) != TableMetadataFlags.None;

        public static bool HasFlag(this IBindingMetadata metadata, BindingMetadataFlags flag) => (metadata.Flags & flag) == flag;
        public static bool HasAnyFlag(this IBindingMetadata metadata, BindingMetadataFlags flag) => (metadata.Flags & flag) != BindingMetadataFlags.None;
        public static bool HasFlag(this IBindingMetadata metadata, RelationMetadataFlags flag) => metadata.Relation.HasFlag(flag);
        public static bool HasAnyFlag(this IBindingMetadata metadata, RelationMetadataFlags flag) => metadata.Relation.HasAnyFlag(flag);

        public static bool HasFlag(this IReferenceMetadata metadata, ReferenceMetadataFlags flag) => (metadata.Flags & flag) == flag;
        public static bool HasAnyFlag(this IReferenceMetadata metadata, ReferenceMetadataFlags flag) => (metadata.Flags & flag) != ReferenceMetadataFlags.None;
        public static bool HasFlag(this IReferenceMetadata metadata, RelationMetadataFlags flag) => metadata.Relation.HasFlag(flag);
        public static bool HasAnyFlag(this IReferenceMetadata metadata, RelationMetadataFlags flag) => metadata.Relation.HasAnyFlag(flag);

        public static bool HasFlag(this IReference metadata, ReferenceFlags flag) => (metadata.Flags & flag) == flag;
        public static bool HasAnyFlag(this IReference metadata, ReferenceFlags flag) => (metadata.Flags & flag) != ReferenceFlags.None;
    }
}
