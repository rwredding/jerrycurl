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
    public class Relation3 : IRelation3
    {
        public RelationHeader Header { get; }
        public IField2 Model => this.Source.Model;
        public IField2 Source { get; }

        public Relation3(IField2 source, RelationHeader header)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Header = header ?? throw new ArgumentNullException(nameof(header));
        }

        public IRelationReader GetReader() => new RelationReader(this);
        public IDataReader GetDataReader() => null; // new RelationDataReader(this.GetReader(), null);

        public IEnumerable<IField2[]> Body
        {
            get
            {
                using IRelationReader reader = this.GetReader();

                while (reader.Read())
                {
                    IField2[] buffer = new IField2[reader.Degree];

                    reader.CopyTo(buffer, buffer.Length);

                    yield return buffer;
                }
            }
        }


        #region " IField implementation "
        FieldType2 IField2.Type => this.Source.Type;
        FieldIdentity IField2.Identity => this.Source.Identity;
        IRelationMetadata IField2.Metadata => this.Source.Metadata;
        object IField2.Value => this.Source.Value;
        object IField2.CurrentValue
        {
            get => this.Source.CurrentValue;
            set => this.Source.CurrentValue = value;
        }
        void IField2.Bind() => this.Source.Bind();
        #endregion


        #region " Equality "

        public bool Equals(IField2 other) => Equality.Combine(this.Source, other, m => m.Identity, m => m.Model);
        public override bool Equals(object obj) => (obj is IField2 other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Source.Identity, this.Source.Model);

        #endregion

        public override string ToString() => this.Header.ToString();
    }
}
