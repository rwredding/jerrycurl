using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Internal;
using Jerrycurl.Relations.V11.Internal.Compilation;
using Jerrycurl.Relations.Metadata;
using System;
using System.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11
{
    [DebuggerDisplay("{Identity.Name}: {ToString(),nq}")]
    internal class Field2<TValue, TParent> : IField2
    {
        public FieldIdentity Identity { get; }
        public object Value { get; private set; }
        public IField2 Model { get; }
        public FieldType2 Type { get; }
        public IRelationMetadata Metadata { get; }

        private readonly Action<TValue> binder;
        private bool hasChanged = false;
        private object currentValue;

        public Field2(string name, IRelationMetadata metadata, TParent parent, int index, TValue value, Delegate binder, IField2 model, FieldType2 type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            this.Identity = new FieldIdentity(metadata.Identity, name);
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Value = this.currentValue = value;
            this.Type = type;
            this.Metadata = metadata;

            if (binder is FieldBinder<TParent, TValue> typedBinder)
                this.binder = v => typedBinder(parent, index, v);
        }
        public object CurrentValue
        {
            get => this.currentValue;
            set
            {
                this.currentValue = value;
                this.hasChanged = true;
            }
        }

        public void Update()
        {
            if (!this.hasChanged)
                return;

            //if (this.writer == null)
            //    throw BindingException.FromField(this, "Field is not bindable.");

            try
            {
                this.binder((TValue)this.currentValue);
                this.Value = this.currentValue;
                this.hasChanged = false;
            }
            catch (NotIndexableException)
            {
                //throw BindingException.FromField(this, "Property has no indexer.");
            }
            catch (NotWritableException)
            {
                //throw BindingException.FromField(this, "Property has no setter.");
            }
            catch (Exception ex)
            {
                //throw BindingException.FromField(this, innerException: ex);
            }
        }

        public bool Equals(IField2 other) => Equality.Combine(this, other, m => m.Model, m => m.Identity);
        public override bool Equals(object obj) => (obj is IField2 other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Model, this.Identity);

        public override string ToString() => this.Value != null ? this.Value.ToString() : "<null>";
    }
}
