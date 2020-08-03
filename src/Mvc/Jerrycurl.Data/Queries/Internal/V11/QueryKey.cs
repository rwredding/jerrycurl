using System;
using System.Collections.Generic;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryKey<T> : IEquatable<QueryKey<T>>
        where T : IEquatable<T>
    {
        public ISchema Schema { get; }
        public IReadOnlyList<T> Columns { get; }

        public QueryKey(ISchema schema, IReadOnlyList<T> values)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Columns = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool Equals(QueryKey<T> other) => (Equality.Combine(this.Schema, other?.Schema) && Equality.CombineAll(this.Columns, other?.Columns));
        public override bool Equals(object obj) => (obj is QueryKey<T> other && this.Equals(other));
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(this.Schema);
            hashCode.Add(this.Columns);

            return hashCode.ToHashCode();
        }
    }
}
