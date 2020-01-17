using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Internal;
using Jerrycurl.Relations.Metadata;
using System;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    internal class Field<TValue, TParent> : IField
    {
        public FieldIdentity Identity { get; }
        public object Value { get; private set; }
        public IField Model { get; }
        public FieldType Type => FieldType.Value;

        private readonly Action<TValue> binder;

        public Field(string name, MetadataIdentity metadata, TValue value, TParent parent, Action<TParent, int, TValue> binder, int index, IField model)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            this.Identity = new FieldIdentity(metadata, name);
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Value = value;

            if (binder != null)
                this.binder = v => binder(parent, index, v);
        }

        public void Bind(object newValue)
        {
            if (this.binder == null)
                throw BindingException.FromField(this, "Field is not bindable.");

            try
            {
                this.binder((TValue)newValue);
                this.Value = newValue;
            }
            catch (NotIndexableException)
            {
                throw BindingException.FromField(this, "Property has no indexer.");
            }
            catch (NotWritableException)
            {
                throw BindingException.FromField(this, "Property has no setter.");
            }
            catch (Exception ex)
            {
                throw BindingException.FromField(this, innerException: ex);
            }
        }

        public bool Equals(IField other) => Equality.Combine(this, other, m => m.Model, m => m.Identity);
        public override bool Equals(object obj) => (obj is IField other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Model, this.Identity);

        public override string ToString()
        {
            return this.Identity.Name + "=" + (this.Value?.ToString() ?? "<null>");
        }
    }
}
