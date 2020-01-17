using Jerrycurl.Diagnostics;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Mvc
{
    internal class ProcLookupKey : IEquatable<ProcLookupKey>
    {
        public string Prefix { get; }
        public IProjectionIdentity Identity { get; }
        public MetadataIdentity Metadata { get; }
        public IField Field { get; }
        public bool HasValue { get; }

        public ProcLookupKey(string prefix, IProjectionIdentity identity, MetadataIdentity metadata, IField field)
        {
            this.Prefix = prefix;
            this.Identity = identity;
            this.Metadata = metadata;
            this.Field = field;
            this.HasValue = (identity != null || metadata != null || field != null);

            if (string.IsNullOrWhiteSpace(this.Prefix))
                throw new ArgumentException("Prefix cannot be empty.");
        }

        public bool Equals(ProcLookupKey other)
        {
            if (other == null)
                return false;
            else if (this.HasValue)
                return Equality.Combine(this, other, m => m.Identity, m => m.Metadata, m => m.Field, m => m.Prefix);

            return object.ReferenceEquals(this, other);
        }
        public override bool Equals(object obj) => (obj is ProcLookupKey other && this.Equals(other));
        public override int GetHashCode()
        {
            int hashCode = HashCode.Combine(this.Identity, this.Metadata, this.Field, this.Prefix);

            if (this.HasValue)
                return hashCode;

            return HashCode.Combine(hashCode, base.GetHashCode());
        }
    }
}
