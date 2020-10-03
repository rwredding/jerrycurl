using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.V11
{
    public class RelationAttribute
    {
        public IRelationMetadata Metadata { get; }
        public MetadataIdentity Identity => this.Metadata.Identity;
        public ISchema Schema => this.Identity.Schema;
        public string Name => this.Identity.Name;

        public RelationAttribute(IRelationMetadata metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public RelationAttribute(ISchema schema, string attributeName)
            : this(schema.GetMetadata<IRelationMetadata>(attributeName))
        {
            
        }
    }
}
