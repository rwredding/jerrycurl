using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal
{
    internal class MetadataEntry
    {
        public Schema Schema { get; }
        public IMetadataBuilder Builder { get; }

        public IRelationMetadata Metadata { get; set; }
        public IReadOnlyList<IRelationMetadata> Attributes { get; set; }

        public MetadataEntry(Schema schema, IMetadataBuilder builder)
        {
            this.Schema = schema;
            this.Builder = builder;
        }
    }
}
