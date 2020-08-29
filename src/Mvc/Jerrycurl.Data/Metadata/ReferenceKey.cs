using Jerrycurl.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Metadata
{
    internal class ReferenceKey : IReferenceKey
    {
        public string Name { get; set; }
        public string Other { get; set; }
        public ReferenceKeyType Type { get; set; }
        public List<ReferenceMetadata> Properties { get; set; } = new List<ReferenceMetadata>();
        public bool IsPrimaryKey { get; set; }

        IReadOnlyList<IReferenceMetadata> IReferenceKey.Properties => this.Properties;

        public bool Equals(IReferenceKey other) => Equality.Combine(this, other, m => m.Name, m => m.Other);
        public override bool Equals(object obj) => (obj is IReferenceKey other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Name, this.Other);

        public override string ToString()
        {
            string propNames = string.Join(", ", this.Properties.Select(m => m.Identity.Name));

            return this.Type switch
            {
                ReferenceKeyType.CandidateKey when this.IsPrimaryKey => $"PK: {this.Name}({propNames})",
                ReferenceKeyType.ForeignKey => $"FK: {this.Name}({propNames}) -> {this.Other}",
                _ => $"CK: {this.Name}({propNames})",
            };
        }
    }
}
