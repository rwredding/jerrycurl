namespace Jerrycurl.Relations.Metadata
{
    public interface IMetadataBuilderContext
    {
        MetadataIdentity Identity { get; }
        IMetadataNotation Notation { get; }
        ISchema Schema { get; }

        void AddMetadata<TMetadata>(TMetadata metadata) where TMetadata : IMetadata;
        TMetadata GetMetadata<TMetadata>(string name) where TMetadata : IMetadata;
    }
}
