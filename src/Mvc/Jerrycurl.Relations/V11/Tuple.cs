using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.V11.Language;
using Jerrycurl.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11
{
    [DebuggerDisplay("{ToString(),nq}")]
    internal class Tuple2 : ITuple2
    {
        private readonly IField2[] buffer;

        public int Degree { get; }
        public int Count => this.Degree;

        public Tuple2(IField2[] buffer)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            this.Degree = buffer.Length;
        }

        public IField2 this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException("Index must be a non-negative value.");
                else if (index >= this.Degree)
                    throw new IndexOutOfRangeException("Index must be within the degree of the tuple.");

                return this.buffer[index];
            }
        }

        public bool Equals(ITuple2 other) => Equality.CombineAll(this, other);

        public IEnumerator<IField2> GetEnumerator()
        {
            for (int i = 0; i < this.Degree; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public override bool Equals(object obj) => (obj is ITuple2 tup && this.Equals(tup));

        public override int GetHashCode() => HashCode.CombineAll(this.buffer);

        internal static string Format(IEnumerable<IField2> fields)
        {
            StringBuilder s = new StringBuilder();

            s.Append('(');
            s.AppendJoin(", ", fields);
            s.Append(')');

            return s.ToString();
        }

        public override string ToString() => Format(this);
    }
}
