using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Internal.V11.Caching;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Internal.V11
{
    public class Relation2 : IRelation2
    {
        public RelationIdentity2 Identity { get; }
        public IField2 Model => this.Source.Model;
        public IField2 Source { get; }

        public Relation2(IField2 source, RelationIdentity2 identity)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public IRelationReader GetReader() => this.GetReaderInternal();
        internal RelationReader GetReaderInternal() => new RelationReader(this);
        public IDataReader GetDataReader() => null; // new RelationDataReader(this.GetReader(), null);

        public IEnumerable<IField2[]> Body
        {
            get
            {
                using RelationReader reader = this.GetReaderInternal();

                IField2[] buffer = reader.Buffer;

                while (reader.Read())
                {
                    IField2[] fields = new IField2[this.Identity.Heading.Count];

                    Array.Copy(buffer, fields, fields.Length);

                    yield return fields;
                }
            }
        }


        #region " IField implementation "
        FieldType IField2.Type => this.Source.Type;
        FieldIdentity IField2.Identity => this.Source.Identity;

        object IField2.Value
        {
            get => this.Source.Value;
            set => this.Source.Value = value;
        }
        void IField2.Bind() => this.Source.Bind();
        #endregion


        #region " Equality "

        public bool Equals(IField2 other) => Equality.Combine(this.Source, other, m => m.Identity, m => m.Model);
        public override bool Equals(object obj) => (obj is IField2 other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Source.Identity, this.Source.Model);

        #endregion

        public override string ToString() => this.Identity.Schema + "(" + string.Join(", ", this.Identity.Heading) + ")";

        public static Relation2 Create<TModel>(ISchemaStore store, TModel value, params string[] heading)
        {
            IField2 source = Model2.Create(store, value);
            ISchema schema = source.Identity.Schema;
            IEnumerable<MetadataIdentity> metadata = heading.Select(a => new MetadataIdentity(schema, a));
            RelationIdentity2 identity = new RelationIdentity2(schema, metadata);

            return new Relation2(source, identity);
        }
    }
}
