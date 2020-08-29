using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Internal;
using Jerrycurl.Relations.Internal.V11.Compilation;
using Jerrycurl.Relations.Metadata;
using System;
using System.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Internal.V11
{
    [DebuggerDisplay("{Identity.Name}: {ToString(),nq}")]
    internal class Field2<TValue, TParent> : IField2
    {
        public FieldIdentity Identity { get; }
        public object Value { get; set; }
        public IField2 Model { get; }
        public FieldType2 Type { get; }

        private readonly Action<TValue> writer;

        public Field2(string name, MetadataIdentity metadata, TParent parent, int index, TValue value, ObjectBinder<TParent, TValue> writer, IField2 model, FieldType2 type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            this.Identity = new FieldIdentity(metadata, name);
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Value = value;
            this.Type = type;

            if (writer != null)
                this.writer = v => writer(parent, index, v);
        }

        public void Bind()
        {
            //if (this.writer == null)
            //    throw BindingException.FromField(this, "Field is not bindable.");

            try
            {
                this.writer((TValue)this.Value);
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
