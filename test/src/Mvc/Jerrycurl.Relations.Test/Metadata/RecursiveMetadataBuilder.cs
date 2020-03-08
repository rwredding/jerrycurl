using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Test.Metadata
{
    public class RecursiveMetadataBuilder : IMetadataBuilder<CustomMetadata>
    {
        public CustomMetadata GetMetadata(IMetadataBuilderContext context)
        {
            MetadataIdentity parentIdentity = context.Identity.Pop();

            if (parentIdentity != null)
                return context.Schema.GetMetadata<CustomMetadata>(parentIdentity.Name);

            return null;
        }

        public void Initialize(IMetadataBuilderContext context)
        {

        }
    }
}
