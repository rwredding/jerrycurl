using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.V11.Internal.Caching;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11
{
    public class Relation2 : IRelation2
    {
        public RelationHeader Header { get; }
        public IField2 Model => this.Source.Model;
        public IField2 Source { get; }

        public Relation2(IField2 source, RelationHeader header)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Header = header ?? throw new ArgumentNullException(nameof(header));
        }

        public IRelationReader GetReader() => new RelationReader(this);
        public IDataReader GetDataReader() => null; // new RelationDataReader(this.GetReader(), null);

        public IEnumerable<ITuple2> Body
        {
            get
            {
                using IRelationReader reader = this.GetReader();

                while (reader.Read())
                {
                    IField2[] buffer = new IField2[reader.Degree];

                    reader.CopyTo(buffer, buffer.Length);

                    yield return new Tuple2(buffer);
                }
            }
        }


        #region " Equality "

        public bool Equals(IField2 other) => Equality.Combine(this.Source, other, m => m.Identity, m => m.Model);
        public override bool Equals(object obj) => (obj is IField2 other && this.Equals(other));
        public override int GetHashCode() => HashCode.Combine(this.Source.Identity, this.Source.Model);

        #endregion

        public override string ToString() => this.Header.ToString();
    }
}
