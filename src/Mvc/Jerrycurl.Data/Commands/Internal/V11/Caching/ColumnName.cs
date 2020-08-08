using System;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Commands.Internal.V11.Caching
{
    internal class ColumnName : IEquatable<ColumnName>
    {
        public MetadataIdentity Metadata { get; }
        public ColumnInfo ColumnInfo { get; }

        public ColumnName(MetadataIdentity metadata, ColumnInfo columnInfo)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.ColumnInfo = columnInfo ?? throw new ArgumentNullException(nameof(columnInfo));
        }

        public bool Equals(ColumnName other) => Equality.Combine(this, other, m => m.Metadata, m => m.ColumnInfo.Type, m => m.ColumnInfo.TypeName, m => m.ColumnInfo.Index);
        public override bool Equals(object obj) => (obj is ColumnInfo other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Metadata, this.ColumnInfo.Type, this.ColumnInfo.TypeName, this.ColumnInfo.Index);
    }
}
