using Jerrycurl.Collections;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Linq;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class FuncTableKey : IEquatable<FuncTableKey>
    {
        public FuncColumnKey[] Columns { get; }

        public FuncTableKey(MetadataIdentity[] metadata, TableIdentity heading)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (heading == null)
                throw new ArgumentNullException(nameof(heading));

            this.Columns = metadata.Zip(heading.Columns).Select(t => new FuncColumnKey(t.l, t.r)).ToArray();
        }

        public bool Equals(FuncTableKey other) => Equality.CombineAll(this.Columns, other?.Columns);
        public override int GetHashCode() => HashCode.CombineAll(this.Columns);
        public override bool Equals(object obj) => (obj is FuncTableKey other && this.Equals(other));
    }
}
