using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FuncColumnKey : IEquatable<FuncColumnKey>
    {
        public MetadataIdentity Metadata { get; }
        public ColumnIdentity Column { get; }

        public FuncColumnKey(MetadataIdentity metadata, ColumnIdentity column)
        {
            this.Metadata = metadata;
            this.Column = column;
        }

        public bool Equals(FuncColumnKey other) => Equality.Combine(this, other, m => m.Metadata, m => m.Column?.Type, m => m.Column?.TypeName);
        public override int GetHashCode() => HashCode.Combine(this.Metadata, this.Column?.Type, this.Column?.TypeName);
        public override bool Equals(object obj) => (obj is FuncColumnKey key && this.Equals(key));
    }
}
