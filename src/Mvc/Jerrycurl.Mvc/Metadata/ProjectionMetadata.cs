using System;
using System.Collections.Generic;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Metadata
{
    internal class ProjectionMetadata : IProjectionMetadata
    {
        public MetadataIdentity Identity => this.Relation.Identity;
        public Type Type => this.Relation.Type;

        public ITableMetadata Table { get; }
        public IReferenceMetadata Reference { get; }
        public IRelationMetadata Relation { get; }
        public Lazy<IReadOnlyList<ProjectionMetadata>> Properties { get; set; }
        public IProjectionMetadata Item { get; set; }
        public IProjectionMetadata List { get; set; }
        public IProjectionMetadata Parameter { get; set; }
        public ProjectionMetadataFlags Flags { get; set; }

        IReadOnlyList<IProjectionMetadata> IProjectionMetadata.Properties => this.Properties.Value;

        public ProjectionMetadata(IRelationMetadata relation)
        {
            this.Relation = relation ?? throw new ArgumentNullException(nameof(relation));
            this.Table = relation.Identity.GetMetadata<ITableMetadata>();
            this.Reference = relation.Identity.GetMetadata<IReferenceMetadata>();
        }

        public override string ToString() => $"IProjectionMetadata: {this.Identity}";
    }
}
