using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Metadata
{
    internal class JsonMetadata : IJsonMetadata
    {
        public MetadataIdentity Identity => this.Relation.Identity;
        public string Path { get; set; }
        public IJsonMetadata MemberOf { get; set; }
        public bool IsRoot { get; set; }
        public IRelationMetadata Relation { get; }

        public JsonMetadata(IRelationMetadata relation)
        {
            this.Relation = relation;
        }

        public bool Equals(IJsonMetadata other) => Equality.Combine(this.Identity, other?.Identity);
        public override int GetHashCode() => this.Identity.GetHashCode();
        public override bool Equals(object obj) => (obj is IJsonMetadata other && this.Equals(other));

        public override string ToString() => $"IJsonMetadata: {this.Identity}";
    }
}
