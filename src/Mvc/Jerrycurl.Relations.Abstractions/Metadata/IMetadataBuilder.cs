namespace Jerrycurl.Relations.Metadata
{
    public interface IMetadataBuilder
    {
        void Initialize(IMetadataBuilderContext context);
    }

    public interface IMetadataBuilder<TMetadata> : IMetadataBuilder
         where TMetadata : IMetadata
    {
        TMetadata GetMetadata(IMetadataBuilderContext context);
    }
}
