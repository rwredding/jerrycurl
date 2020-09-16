using System;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Commands.Internal.Caching
{
    internal class ColumnName : IEquatable<ColumnName>
    {
        public MetadataIdentity Metadata { get; }
        public ColumnMetadata Column { get; }

        public ColumnName(MetadataIdentity metadata, ColumnMetadata column)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.Column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public bool Equals(ColumnName other) => Equality.Combine(this, other, m => m.Metadata, m => m.Column.Type, m => m.Column.TypeName, m => m.Column.Index);
        public override bool Equals(object obj) => (obj is ColumnMetadata other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Metadata, this.Column.Type, this.Column.TypeName, this.Column.Index);
    }
}
