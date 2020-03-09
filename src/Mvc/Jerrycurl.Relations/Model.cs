using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Diagnostics;

namespace Jerrycurl.Relations
{
    [DebuggerDisplay("{Identity.Name}: {ToString(),nq}")]
    public class Model : IField
    {
        public FieldIdentity Identity { get; }
        public object Value { get; }
        public FieldType Type => FieldType.Model;

        IField IField.Model => this;

        public Model(ISchema schema, object value)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            this.Identity = new FieldIdentity(new MetadataIdentity(schema, schema.Notation.Model()), schema.Notation.Model());
            this.Value = value;
        }

        public void Bind(object newValue) => throw BindingException.FromField(this, "Models are not not bindable due to having no container.");

        public bool Equals(IField other) => Equality.Combine(this, other, m => m.Identity, m => m.Value);
        public override bool Equals(object obj) => (obj is IField other && this.Equals(other));
        public override int GetHashCode() => this.Identity.GetHashCode();

        public static Model Create<TModel>(ISchemaStore store, TModel value)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            return new Model(store.GetSchema(typeof(TModel)), value);
        }

        public override string ToString() => this.Value != null ? this.Value.ToString() : "<null>";
    }
}
