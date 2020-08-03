using System;
using System.Collections;
using System.Collections.Generic;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class AggregateIdentity : IEquatable<AggregateIdentity>, IEnumerable<MetadataIdentity>
    {
        public ISchema Schema { get; set; }
        private readonly HashSet<MetadataIdentity> values = new HashSet<MetadataIdentity>();

        public void Add(MetadataIdentity value) => this.values.Add(value);
        public void Add(IEnumerable<MetadataIdentity> values)
        {
            foreach (MetadataIdentity metadata in values)
                this.Add(metadata);
        }

        public bool Equals(AggregateIdentity other) => Equality.CombineAll(this.values, other?.values);
        public override bool Equals(object obj) => (obj is AggregateIdentity other && this.Equals(other));
        public override int GetHashCode() => HashCode.CombineAll(this.values);

        public IEnumerator<MetadataIdentity> GetEnumerator() => this.values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
