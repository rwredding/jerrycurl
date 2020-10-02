using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Diagnostics;
using Jerrycurl.Relations.Internal.V11.Language;

namespace Jerrycurl.Relations.Internal.V11
{
    [DebuggerDisplay("{Identity.Name}: {ToString(),nq}")]
    public class Model2 : IField2
    {
        public FieldIdentity Identity { get; }
        public object Value { get; }
        public FieldType2 Type { get; } = FieldType2.Model;
        public object CurrentValue { get; set; }
        public IRelationMetadata Metadata { get; }

        IField2 IField2.Model => this;

        public Model2(ISchema schema, object value)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            this.Identity = new FieldIdentity(new MetadataIdentity(schema, schema.Notation.Model()), schema.Notation.Model());
            this.Value = this.CurrentValue = value;
            this.Metadata = schema.GetMetadata<IRelationMetadata>() ?? throw new InvalidOperationException("Metadata not found.");
        }

        public void Update() => throw new NotSupportedException("Models are not not bindable due to having no container.");

        public bool Equals(IField2 other) => Equality.Combine(this, other, m => m.Identity, m => m.Value);
        public override bool Equals(object obj) => (obj is IField2 other && this.Equals(other));
        public override int GetHashCode() => this.Identity.GetHashCode();

        public override string ToString() => this.Value != null ? this.Value.ToString() : "<null>";
    }
}
