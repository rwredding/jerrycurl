using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal
{
    internal struct CompositeKey<T1> : IEquatable<CompositeKey<T1>>
    {
        private readonly T1 item1;

        public CompositeKey(T1 item1)
        {
            this.item1 = item1;
        }

        public bool Equals(CompositeKey<T1> other) => Equality.Combine(this.item1, other.item1);
        public override bool Equals(object obj) => (obj is CompositeKey<T1> c && this.Equals(c));
        public override int GetHashCode() => HashCode.Combine(this.item1);

        public override string ToString() => $"({this.item1})";
    }

    internal struct CompositeKey<T1, T2> : IEquatable<CompositeKey<T1, T2>>
    {
        private readonly T1 item1;
        private readonly T2 item2;

        public CompositeKey(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public bool Equals(CompositeKey<T1, T2> other) => Equality.Combine(this, other, m => m.item1, m => m.item2);
        public override bool Equals(object obj) => (obj is CompositeKey<T1, T2> other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.item1, this.item2);

        public override string ToString() => $"({this.item1}, {this.item2})";
    }

    internal struct CompositeKey<T1, T2, T3> : IEquatable<CompositeKey<T1, T2, T3>>
    {
        private readonly T1 item1;
        private readonly T2 item2;
        private readonly T3 item3;

        public CompositeKey(T1 item1, T2 item2, T3 item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }

        public bool Equals(CompositeKey<T1, T2, T3> other) => Equality.Combine(this, other, m => m.item1, m => m.item2, m => m.item3);
        public override bool Equals(object obj) => (obj is CompositeKey<T1, T2, T3> other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.item1, this.item2, this.item3);

        public override string ToString() => $"({this.item1}, {this.item2}, {this.item3})";
    }

    internal struct CompositeKey<T1, T2, T3, T4> : IEquatable<CompositeKey<T1, T2, T3, T4>>
    {
        private readonly T1 item1;
        private readonly T2 item2;
        private readonly T3 item3;
        private readonly T4 item4;

        public CompositeKey(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
        }

        public bool Equals(CompositeKey<T1, T2, T3, T4> other) => Equality.Combine(this, other, m => m.item1, m => m.item2, m => m.item3, m => m.item4);
        public override bool Equals(object obj) => (obj is CompositeKey<T1, T2, T3, T4> other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.item1, this.item2, this.item3, this.item4);

        public override string ToString() => $"({this.item1}, {this.item2}, {this.item3}, {this.item4})";
    }

    internal struct CompositeKey<T1, T2, T3, T4, TRest> : IEquatable<CompositeKey<T1, T2, T3, T4, TRest>>
    {
        private readonly T1 item1;
        private readonly T2 item2;
        private readonly T3 item3;
        private readonly T4 item4;
        private readonly TRest item5;

        public CompositeKey(T1 item1, T2 item2, T3 item3, T4 item4, TRest item5)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
            this.item5 = item5;
        }

        public bool Equals(CompositeKey<T1, T2, T3, T4, TRest> other) => Equality.Combine(this, other, m => m.item1, m => m.item2, m => m.item3, m => m.item4, m => m.item5);
        public override bool Equals(object obj) => (obj is CompositeKey<T1, T2, T3, T4, TRest> other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.item1, this.item2, this.item3, this.item4, this.item5);

        public override string ToString() => $"({this.item1}, {this.item2}, {this.item3}, {this.item4}, {this.item5})";
    }
}
