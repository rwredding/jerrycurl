using System;
using System.Collections.Generic;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    internal class TableMetadata : ITableMetadata
    {
        public MetadataIdentity Identity => this.Relation.Identity;
        public IRelationMetadata Relation { get; }

        public TableMetadataFlags Flags { get; set; }
        public TableMetadata MemberOf { get; set; }
        public IReadOnlyList<string> TableName { get; set; }
        public Lazy<IReadOnlyList<TableMetadata>> Properties { get; set; }
        public ITableMetadata Item { get; set; }
        public string ColumnName { get; set; }

        IReadOnlyList<ITableMetadata> ITableMetadata.Properties => this.Properties?.Value;
        ITableMetadata ITableMetadata.MemberOf => this.MemberOf;

        public TableMetadata(IRelationMetadata relation)
        {
            this.Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }

        public bool Equals(ITableMetadata other) => Equality.Combine(this.Identity, other?.Identity);
        public override int GetHashCode() => this.Identity.GetHashCode();
        public override bool Equals(object obj) => (obj is ITableMetadata other && this.Equals(other));

        public override string ToString() => $"ITableMetadata: {this.Identity}";
    }
}
