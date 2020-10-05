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
        public IField2 Model { get; }
        public FieldType2 Type { get; }
        public IRelationMetadata Metadata { get; }
        public FieldData<TValue, TParent> Data { get; }
        public bool HasChanged { get; private set; }

        private object value;

        public Field2(string name, IRelationMetadata metadata, FieldData<TValue, TParent> data, IField2 model, FieldType2 type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            this.Identity = new FieldIdentity(metadata.Identity, name);
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Type = type;
            this.Metadata = metadata;
            this.value = data.Value;
        }

        public object Snapshot
        {
            get => this.value;
            set
            {
                this.value = value;
                this.HasChanged = true;
            }
        }

        public void Commit()
        {
            if (!this.HasChanged)
                return;

            //if (this.writer == null)
            //    throw BindingException.FromField(this, "Field is not bindable.");

            try
            {
                TValue typedValue = (TValue)this.Snapshot;

                this.Data.Update((TValue)this.Snapshot);
                this.HasChanged = false;
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

        public void Rollback()
        {
            this.value = this.Data.Value;
            this.HasChanged = false;
        }

        IFieldData IField2.Data => this.Data;

        public bool Equals(IField2 other) => Equality.Combine(this, other, m => m.Model, m => m.Identity);
        public override bool Equals(object obj) => (obj is IField2 other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Model, this.Identity);

        public override string ToString() => this.Snapshot != null ? this.Snapshot.ToString() : "<null>";
    }
}
