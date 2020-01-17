using Jerrycurl.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class KeyNode
    {
        public IList<MetadataNode> Value { get; set; }
        public IReference Reference { get; set; }
        public int Index { get; set; }

        public IList<Type> Type { get; set; }
        public int ParentIndex { get; set; }
        public int? ChildIndex { get; set; }
        public IBindingMetadata Metadata { get; set; }

        public override string ToString() => this.Reference.Key.ToString();

        public KeyNode()
        {

        }

        public KeyNode(IReference reference)
        {
            this.Reference = reference ?? throw new ArgumentNullException(nameof(reference));
            this.Type = this.GetKeyType(reference).ToList();
            this.Metadata = reference.Metadata.Identity.GetMetadata<IBindingMetadata>();
        }

        private IEnumerable<Type> GetKeyType(IReference reference)
        {
            IReferenceKey candidateKey = reference.Key.Type == ReferenceKeyType.CandidateKey ? reference.Key : reference.Other.Key;

            return candidateKey.Properties.Select(m => Nullable.GetUnderlyingType(m.Type) ?? m.Type);
        }
    }
}
