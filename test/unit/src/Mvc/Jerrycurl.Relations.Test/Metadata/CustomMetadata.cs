using Jerrycurl.Relations.Metadata;
using System;

namespace Jerrycurl.Relations.Test.Metadata
{
    public class CustomMetadata : IMetadata
    {
        public MetadataIdentity Identity { get; }

        public CustomMetadata(MetadataIdentity identity)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }
    }
}
