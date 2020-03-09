using Jerrycurl.Diagnostics;
using System;
using System.Collections.Generic;
using Jerrycurl.Relations.Metadata;
using System.Collections;
using System.Linq;
using Jerrycurl.Relations.Internal;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations
{
    public sealed class Relation : IRelation
    {
        public RelationIdentity Identity { get; }
        public IField Model { get; }
        public IField Source { get; }

        FieldType IField.Type => this.Source.Type;
        FieldIdentity IField.Identity => this.Source.Identity;
        object IField.Value => this.Source.Value;

        public Relation(object model, RelationIdentity identity)
        {
            this.Model = this.Source = new Model(identity.Schema, model);
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public Relation(object model, IEnumerable<MetadataIdentity> heading)
        {
            if (heading == null || !heading.Any())
                throw new ArgumentException("Heading cannot be empty when no schema is supplied.", nameof(heading));

            this.Identity = new RelationIdentity(heading.First().Schema, heading);
            this.Model = this.Source = new Model(this.Identity.Schema, model);
        }

        public Relation(IField source, RelationIdentity identity)
        {
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Model = source.Model;
        }

        public Relation(object model, ISchema schema, params string[] heading)
            : this(model, new RelationIdentity(schema, heading?.Select(n => new MetadataIdentity(schema, n))))
        {

        }
        public Relation(object model, ISchema schema, IEnumerable<MetadataIdentity> heading)
            : this(model, new RelationIdentity(schema, heading))
        {

        }

        public Relation(IField source, IEnumerable<MetadataIdentity> heading)
            : this(source, new RelationIdentity(source?.Identity?.Schema, heading))
        {

        }

        public Relation(IField source, params string[] heading)
            : this(source, new RelationIdentity(source?.Identity?.Schema, heading?.Select(n => new MetadataIdentity(source?.Identity?.Schema, n))))
        {

        }

        void IField.Bind(object newValue) => this.Source.Bind(newValue);

        public bool Equals(IField other) => Equality.Combine(this.Source, other, m => m.Identity, m => m.Model);
        public override bool Equals(object obj) => (obj is IField other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Source.Identity, this.Source.Model);

        public override string ToString() => this.Identity.Schema + "(" + string.Join(", ", this.Identity.Heading) + ")";

        public IEnumerator<ITuple> GetEnumerator()
        {
            FuncDescriptor descriptor = FuncCache.GetDescriptor(this.Identity, this.Source.Identity.Metadata);

            IEnumerator[] enumerators = new IEnumerator[descriptor.Factories.Length - 1];
            IField[] fields = new IField[descriptor.Degree];

            void newArray()
            {
                IField[] newFields = new IField[fields.Length];

                Array.Copy(fields, newFields, newFields.Length);

                fields = newFields;
            }

            int visibleDegree = descriptor.VisibleDegree;

            descriptor.Factories[0](this.Source, enumerators, fields);

            if (enumerators.Length == 0)
            {
                yield return new Tuple(fields, visibleDegree);
                yield break;
            }

            int i = 0;

            while (i >= 0)
            {
                if (i == enumerators.Length)
                {
                    yield return new Tuple(fields, visibleDegree);

                    newArray();

                    i--;
                }
                else if (this.MoveNextOrThrow(enumerators[i], descriptor.Identity, descriptor.Lists[i]))
                {
                    descriptor.Factories[i + 1](this.Source, enumerators, fields);

                    i++;
                }
                else
                {
                    if (enumerators[i] is IDisposable d)
                        d.Dispose();

                    i--;
                }
            }
        }

        private bool MoveNextOrThrow(IEnumerator enumerator, RelationIdentity relation, MetadataIdentity identity)
        {
            if (enumerator == null)
                return false;

            try
            {
                return enumerator.MoveNext();
            }
            catch (Exception ex)
            {
                throw RelationException.FromRelation(relation, $"Cannot move enumerator for '{identity}': {ex.Message}", ex);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public static Relation Create<TModel>(ISchemaStore store, TModel value, params string[] heading) => new Relation(Relations.Model.Create(store, value), heading);
    }
}
