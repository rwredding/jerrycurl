using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data
{
    public sealed class TableIdentity : IEquatable<TableIdentity>
    {
        public IReadOnlyList<ColumnIdentity> Columns { get; }

        public static TableIdentity FromRecord(IDataRecord adoRecord)
        {
            if (adoRecord == null)
                throw new ArgumentNullException(nameof(adoRecord));

            IEnumerable<ColumnIdentity> columns = Enumerable.Range(0, GetFieldCount(adoRecord)).Select(i => new ColumnIdentity(adoRecord.GetName(i), adoRecord.GetFieldType(i), adoRecord.GetDataTypeName(i), i));

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
