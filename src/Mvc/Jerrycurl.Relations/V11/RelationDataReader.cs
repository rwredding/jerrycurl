using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11
{
    internal class RelationDataReader : IDataReader
    {
        public RelationReader InnerReader { get; }

        private readonly Dictionary<string, int> headingMap;
        private readonly IReadOnlyList<string> headingList;

        public RelationDataReader(RelationReader innerReader, IReadOnlyList<string> heading)
        {
            this.InnerReader = innerReader ?? throw new ArgumentNullException(nameof(innerReader));
            this.headingList = heading ?? throw new ArgumentNullException(nameof(heading));
            this.headingMap = heading.Select((Name, Index) => (Name, Index)).ToDictionary(t => t.Name, t => t.Index);
        }

        public int Depth => 0;
        public bool IsClosed => false;
        public int RecordsAffected => 0;
        public int FieldCount => this.InnerReader.Degree;

        public object this[string name] => this[this.GetOrdinal(name)];
        public object this[int i] => this.InnerReader[i].Snapshot;

        public void Close() { }
        public bool NextResult() => false;
        public bool Read() => this.InnerReader.Read();

        public string GetDataTypeName(int i) => null;
        public Type GetFieldType(int i) => this.InnerReader.Relation.Header.Attributes[i].Metadata.Type;
        public string GetName(int i) => this.headingList[i];
        public int GetOrdinal(string name) => this.headingMap[name];

        public bool IsDBNull(int i) => this.InnerReader[i].Type == FieldType2.Missing;

        public void Dispose() => this.InnerReader.Dispose();

        #region " Get methods "

        public float GetFloat(int i) => (float)this[i];
        public Guid GetGuid(int i) => (Guid)this[i];
        public short GetInt16(int i) => (short)this[i];
        public int GetInt32(int i) => (int)this[i];
        public long GetInt64(int i) => (long)this[i];
        public string GetString(int i) => (string)this[i];
        public object GetValue(int i) => this[i];
        public bool GetBoolean(int i) => (bool)this[i];
        public byte GetByte(int i) => (byte)this[i];
        public char GetChar(int i) => (char)this[i];
        public DateTime GetDateTime(int i) => (DateTime)this[i];
        public decimal GetDecimal(int i) => (decimal)this[i];
        public double GetDouble(int i) => (double)this[i];
        public int GetValues(object[] values)
        {
            int maxLength = Math.Min(values.Length, this.FieldCount);

            for (int i = 0; i < maxLength; i++)
                values[i] = this[i];

            return maxLength;
        }

        #endregion

        #region " Not supported "
        public DataTable GetSchemaTable() => throw new NotSupportedException();
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public IDataReader GetData(int i) => throw new NotSupportedException();
        #endregion
    }
}
