using System;
using System.Data;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data
{
    public sealed class ColumnIdentity : IEquatable<ColumnIdentity>
    {
        public string Name { get; }
        public Type Type { get; }
        public int Index { get; }
        public string TypeName { get; }

        public static ColumnIdentity FromField(IDataRecord dataRecord, int fieldIndex)
        {
            if (dataRecord == null)
                throw new ArgumentNullException(nameof(dataRecord));

            return new ColumnIdentity(dataRecord.GetName(fieldIndex), dataRecord.GetFieldType(fieldIndex), dataRecord.GetDataTypeName(fieldIndex), fieldIndex);
        }

        public ColumnIdentity(string name, Type type, string typeName, int index)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Type = type;
            this.TypeName = typeName;
            this.Index = index;
        }

        public bool Equals(ColumnIdentity other) => Equality.Combine(this, other, ci => ci.Name, ci => ci.Type, ci => ci.TypeName, ci => ci.Index);
        public override bool Equals(object obj) => (obj is ColumnIdentity other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Name, this.Type, this.TypeName, this.Index);
    }
}
