using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data
{
    public sealed class TableIdentity : IEquatable<TableIdentity>
    {
        public IReadOnlyList<ColumnIdentity> Columns { get; }

        public static TableIdentity FromRecord(IDataRecord dataRecord)
        {
            if (dataRecord == null)
                throw new ArgumentNullException(nameof(dataRecord));

            IEnumerable<ColumnIdentity> columns = Enumerable.Range(0, GetFieldCount(dataRecord)).Select(i => ColumnIdentity.FromField(dataRecord, i));

            return new TableIdentity(columns);
        }

        public TableIdentity(IEnumerable<ColumnIdentity> columns)
        {
            this.Columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
        }

        private static int GetFieldCount(IDataRecord record)
        {
            try
            {
                return record.FieldCount;
            }
            catch
            {
                return 0;
            }
        }

        public bool Equals(TableIdentity other) => Equality.CombineAll(this.Columns, other?.Columns);
        public override bool Equals(object obj) => (obj is TableIdentity other && this.Equals(other));
        public override int GetHashCode() => HashCode.CombineAll(this.Columns);
    }
}
