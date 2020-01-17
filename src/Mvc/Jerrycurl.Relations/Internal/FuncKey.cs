using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Internal
{
    internal class FuncKey : IEquatable<FuncKey>
    {
        public RelationIdentity Relation { get; }
        public MetadataIdentity Source { get; }

        public FuncKey(RelationIdentity relation, MetadataIdentity source)
        {
            this.Relation = relation ?? throw new ArgumentNullException(nameof(relation));
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public bool Equals(FuncKey other) => Equality.Combine(this, other, m => m.Relation, m => m.Source);
        public override int GetHashCode() => HashCode.Combine(this.Relation, this.Source);
        public override bool Equals(object obj) => (obj is FuncKey other && this.Equals(other));

    }
}
