using System;

namespace Jerrycurl.Relations.Metadata
{
    internal class MetadataBuilderContext : IMetadataBuilderContext
    {
        public MetadataIdentity Identity { get; }
        public IMetadataNotation Notation => this.Schema.Notation;
        public Schema Schema { get; }

        ISchema IMetadataBuilderContext.Schema => this.Schema;

        public MetadataBuilderContext(MetadataIdentity identity, Schema schema)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public void AddMetadata<TMetadata>(TMetadata metadata)
            where TMetadata : IMetadata
            => this.Schema.AddMetadata(metadata);

        public TMetadata GetMetadata<TMetadata>(string name)
            where TMetadata : IMetadata
            => this.Schema.GetMetadataFromCache<TMetadata>(name);
    }
}
