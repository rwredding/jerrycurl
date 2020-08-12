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
        public ColumnInfo Info { get; }

        public ColumnName(MetadataIdentity metadata, ColumnInfo columnInfo)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.Info = columnInfo ?? throw new ArgumentNullException(nameof(columnInfo));
        }

        public bool Equals(ColumnName other) => Equality.Combine(this, other, m => m.Metadata, m => m.Info.Type, m => m.Info.TypeName, m => m.Info.Index);
        public override bool Equals(object obj) => (obj is ColumnInfo other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Metadata, this.Info.Type, this.Info.TypeName, this.Info.Index);
    }
}
