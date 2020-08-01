using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class ListIdentity : IEquatable<ListIdentity>
    {
        public ISchema Schema { get; }
        public IReadOnlyList<ColumnIdentity> Columns { get; }

        public static ListIdentity FromRecord(ISchema schema, IDataRecord dataRecord)
        {
            if (dataRecord == null)
                throw new ArgumentNullException(nameof(dataRecord));

            IEnumerable<ColumnIdentity> columns = Enumerable.Range(0, GetFieldCount(dataRecord)).Select(i => ColumnIdentity.FromField(dataRecord, i));

            return new ListIdentity(schema, columns);
        }

        public ListIdentity(ISchema schema, IEnumerable<ColumnIdentity> columns)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
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

        public bool Equals(ListIdentity other) => (Equality.Combine(this.Schema, other?.Schema) && Equality.CombineAll(this.Columns, other?.Columns));
        public override bool Equals(object obj) => (obj is ListIdentity other && this.Equals(other));
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(this.Schema);
            hashCode.Add(this.Columns);

            return hashCode.ToHashCode();
        }
    }
}
