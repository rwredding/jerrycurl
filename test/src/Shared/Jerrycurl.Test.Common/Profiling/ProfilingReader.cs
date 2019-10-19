using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Test.Profiling
{
    public class ProfilingReader : DbDataReader
    {
        public DbDataReader InnerReader { get; }

        private readonly Dictionary<int, int> valueReads = new Dictionary<int, int>();
        private readonly Dictionary<int, int> nullReads = new Dictionary<int, int>();

        public ProfilingReader(DbDataReader innerReader)
        {
            this.InnerReader = innerReader ?? throw new ArgumentNullException(nameof(innerReader));
        }


        public override object this[int ordinal] => this.GetValue(ordinal);
        public override object this[string name] => this.GetValue(this.GetOrdinal(name));
        public override int Depth => this.InnerReader.Depth;
        public override int FieldCount => this.InnerReader.FieldCount;
        public override bool HasRows => this.InnerReader.HasRows;
        public override bool IsClosed => this.InnerReader.IsClosed;
        public override int RecordsAffected => this.InnerReader.RecordsAffected;
        public override int VisibleFieldCount => this.InnerReader.VisibleFieldCount;
        public override void Close() => this.InnerReader.Close();

        private void ClearCount()
        {
            this.valueReads.Clear();
            this.nullReads.Clear();
        }

        private void CountNull(int ordinal)
        {
            this.nullReads[ordinal] = (this.nullReads.ContainsKey(ordinal) ? this.nullReads[ordinal] : 0) + 1;
        }

        private void CountValue(int ordinal)
        {
            this.valueReads[ordinal] = (this.valueReads.ContainsKey(ordinal) ? this.valueReads[ordinal] : 0) + 1;
        }

        public override bool GetBoolean(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal) => this.InnerReader.GetDataTypeName(ordinal);

        public override DateTime GetDateTime(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetDouble(ordinal);
        }

        public override Type GetFieldType(int ordinal) => this.InnerReader.GetFieldType(ordinal);
        public override T GetFieldValue<T>(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetFieldValue<T>(ordinal);
        }

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

        public override float GetFloat(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetInt64(ordinal);
        }

        public override string GetName(int ordinal) => this.InnerReader.GetName(ordinal);
        public override int GetOrdinal(string name) => this.InnerReader.GetOrdinal(name);

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return base.GetProviderSpecificFieldType(ordinal);
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            return base.GetProviderSpecificValue(ordinal);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return base.GetProviderSpecificValues(values);
        }

        public override DataTable GetSchemaTable()
        {
            return base.GetSchemaTable();
        }

        public override Stream GetStream(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetStream(ordinal);
        }

        public override string GetString(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetString(ordinal);
        }

        public override bool IsDBNull(int ordinal)
        {
            this.CountNull(ordinal);

            return this.InnerReader.IsDBNull(ordinal);
        }
        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            this.CountNull(ordinal);

            return this.InnerReader.IsDBNullAsync(ordinal, cancellationToken);
        }

        public override TextReader GetTextReader(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetTextReader(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            this.CountValue(ordinal);

            return this.InnerReader.GetValue(ordinal);
        }

        public override int GetValues(object[] values) => throw new NotSupportedException();

        public override object InitializeLifetimeService() => this.InnerReader.InitializeLifetimeService();
        public override bool NextResult()
        {
            this.ClearCount();

            return this.InnerReader.NextResult();
        }
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            this.ClearCount();

            return this.InnerReader.NextResultAsync(cancellationToken);
        }
        public override bool Read()
        {
            this.ClearCount();

            return this.InnerReader.Read();
        }
        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            this.ClearCount();

            return this.InnerReader.ReadAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var ords in this.nullReads)
                {
                    if (ords.Value > 1)
                        throw new InvalidOperationException($"Ordinal '{ords.Key}' was read {ords.Value} times (null read).");
                }

                foreach (var ords in this.valueReads)
                {
                    if (ords.Value > 1)
                        throw new InvalidOperationException($"Ordinal '{ords.Key}' was read {ords.Value} times (value read).");
                }
            }
        }

#if NETCOREAPP3_0
        public override Task CloseAsync() => this.InnerReader.CloseAsync();
#endif

        protected override DbDataReader GetDbDataReader(int ordinal) => throw new NotSupportedException();
        public override IEnumerator GetEnumerator() => ((IEnumerable)this.InnerReader).GetEnumerator();
    }
}
