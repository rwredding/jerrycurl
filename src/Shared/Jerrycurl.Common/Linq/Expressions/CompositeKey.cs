using Jerrycurl.Diagnostics;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Linq.Expressions
{
    internal class CompositeKey : IEquatable<CompositeKey>
    {
        private readonly object[] components;

        public CompositeKey(object[] components)
        {
            this.components = components;
        }

        public bool Equals(CompositeKey other) => Equality.CombineAll(this.components, other?.components);
        public override bool Equals(object obj) => (obj is CompositeKey other && this.Equals(other));
        public override int GetHashCode() => HashCode.CombineAll(this.components);
    }
}
