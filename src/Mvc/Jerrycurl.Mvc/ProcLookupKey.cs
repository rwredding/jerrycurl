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

        public ProcLookupKey(string prefix, IProjectionIdentity identity, MetadataIdentity metadata, IField field)
        {
            this.Prefix = prefix;
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            this.Metadata = metadata;
            this.Field = field;

            if (string.IsNullOrWhiteSpace(this.Prefix))
                throw new ArgumentException("Prefix cannot be empty.");
        }

        public bool Equals(ProcLookupKey other) => Equality.Combine(this, other, m => m.Identity, m => m.Metadata, m => m.Field, m => m.Prefix);
        public override bool Equals(object obj) => (obj is ProcLookupKey other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Identity, this.Metadata, this.Field, this.Prefix);
    }
}
