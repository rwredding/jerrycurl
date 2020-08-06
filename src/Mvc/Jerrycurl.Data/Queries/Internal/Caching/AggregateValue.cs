using System;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class AggregateValue : IEquatable<AggregateValue>, ICacheValue
    {
        public string Name { get; set; }
        public bool IsPrincipal { get; set; }

        public AggregateValue(string name, bool isPrincipal)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.IsPrincipal = isPrincipal;
        }

        public bool Equals(AggregateValue other) => Equality.Combine(this, other, m => m.Name, m => m.IsPrincipal);
        public override bool Equals(object obj) => (obj is AggregateValue other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Name, this.IsPrincipal);
    }
}
