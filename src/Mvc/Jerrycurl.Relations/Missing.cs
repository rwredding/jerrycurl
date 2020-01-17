using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    internal class Missing<TValue> : IField
    {
        public FieldIdentity Identity { get; }
        public object Value { get; }
        public IField Model { get; }
        public FieldType Type => FieldType.Missing;

        public Missing(string name, MetadataIdentity metadata, IField model)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            this.Identity = new FieldIdentity(metadata, name);
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Value = default(TValue);
        }

        public void Bind(object newValue) => throw BindingException.FromField(this, "Missing fields are not bindable due to a null container.");

        public bool Equals(IField other) => Equality.Combine(this, other, m => m.Model, m => m.Identity);
        public override bool Equals(object obj) => (obj is IField field && this.Equals(field));
        public override int GetHashCode() => HashCode.Combine(this.Model, this.Identity);

        public override string ToString() => this.Identity.Name + " = <missing>";
    }
}
