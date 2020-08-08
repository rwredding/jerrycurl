using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Data.Commands.Internal.V11.Caching;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Commands.Internal.Caching
{
    internal class CommandCacheKey : IEquatable<CommandCacheKey>
    {
        public ISchema Schema { get; }
        public IReadOnlyList<ColumnName> Items { get; }

        public CommandCacheKey(ISchema schema, IEnumerable<ColumnName> items)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        }

        public bool Equals(CommandCacheKey other) => (Equality.Combine(this.Schema, other?.Schema) && Equality.CombineAll(this.Items, other?.Items));
        public override bool Equals(object obj) => (obj is CommandCacheKey other && this.Equals(other));
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(this.Schema);
            hashCode.AddRange(this.Items);

            return hashCode.ToHashCode();
        }
    }
}
