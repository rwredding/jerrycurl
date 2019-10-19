using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Test.Metadata
{
    public class RecursiveMetadataBuilder : IMetadataBuilder<CustomMetadata>
    {
        public CustomMetadata GetMetadata(IMetadataBuilderContext context)
        {
            MetadataIdentity parentIdentity = context.Identity.Parent();

            if (parentIdentity != null)
                return context.Schema.GetMetadata<CustomMetadata>(parentIdentity.Name);

            return null;
        }

        public void Initialize(IMetadataBuilderContext context)
        {

        }
    }
}
