using System;
using System.Collections.Generic;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Internal
{
    internal class MetadataKey : IEquatable<MetadataKey>
    {
        public Type MetadataType { get; }
        public string Name { get; }
        public IEqualityComparer<string> Comparer { get; }

        public MetadataKey(Type metadataType, string name, IEqualityComparer<string> comparer)
        {
            this.MetadataType = metadataType ?? throw new ArgumentNullException(nameof(metadataType));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public static MetadataKey FromIdentity<TMetadata>(MetadataIdentity identity) => new MetadataKey(typeof(TMetadata), identity.Name, identity.Schema.Notation.Comparer);

        public bool Equals(MetadataKey other)
        {
            Equality eq = new Equality();

            eq.Add(this.MetadataType, other?.MetadataType);
            eq.Add(this.Name, other?.Name, this.Comparer);

            return eq.ToEquals();
        }
        public override bool Equals(object obj) => (obj is MetadataKey key && this.Equals(key));
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(this.MetadataType);
            hashCode.Add(this.Name, this.Comparer);

            return hashCode.ToHashCode();
        }

        public override string ToString() => this.Name;
    }
}
