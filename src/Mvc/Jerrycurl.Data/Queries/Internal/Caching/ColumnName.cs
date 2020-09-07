﻿using System;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class ColumnName : IEquatable<ColumnName>, IValueName
    {
        public string Name => this.ColumnInfo.Name;
        public ColumnInfo ColumnInfo { get; }

        public ColumnName(ColumnInfo columnInfo)
        {
            this.ColumnInfo = columnInfo ?? throw new ArgumentNullException(nameof(columnInfo));
        }

        public bool Equals(ColumnName other) => Equality.Combine(this, other, m => m.ColumnInfo.Name, m => m.ColumnInfo.Type, m => m.ColumnInfo.TypeName, m => m.ColumnInfo.Index);
        public override bool Equals(object obj) => (obj is ColumnInfo other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.ColumnInfo.Name, this.ColumnInfo.Type, this.ColumnInfo.TypeName, this.ColumnInfo.Index);
    }
}