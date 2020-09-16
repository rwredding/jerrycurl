using System;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class ColumnName : IEquatable<ColumnName>, IValueName
    {
        public string Name => this.Column.Name;
        public ColumnMetadata Column { get; }

        public ColumnName(ColumnMetadata column)
        {
            this.Column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public bool Equals(ColumnName other) => Equality.Combine(this, other, m => m.Column.Name, m => m.Column.Type, m => m.Column.TypeName, m => m.Column.Index);
        public override bool Equals(object obj) => (obj is ColumnMetadata other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Column.Name, this.Column.Type, this.Column.TypeName, this.Column.Index);
    }
}
