using System;
using System.Reflection;
using Jerrycurl.Data.Metadata;
using MySql.Data.MySqlClient;

namespace Jerrycurl.Vendors.MySql.Metadata
{
    public class MySqlContractResolver : IBindingContractResolver
    {
        public int Priority => 1000;

        private MethodInfo GetMySqlReaderMethod(string methodName)
        {
            Type reader = typeof(MySqlDataReader);

            return reader.GetMethod(methodName, new[] { typeof(int) }) ?? throw new InvalidOperationException("No such method.");
        }

        private MethodInfo GetValueReaderProxy(IBindingColumnInfo columnInfo, IBindingValueContract fallback)
        {
            if (columnInfo.Column.Type == typeof(TimeSpan))
                return this.GetMySqlReaderMethod(nameof(MySqlDataReader.GetTimeSpan));
            else if (columnInfo.Column.Type == typeof(sbyte))
                return this.GetMySqlReaderMethod(nameof(MySqlDataReader.GetSByte));
            else if (columnInfo.Column.Type == typeof(ushort))
                return this.GetMySqlReaderMethod(nameof(MySqlDataReader.GetUInt16));
            else if (columnInfo.Column.Type == typeof(uint))
                return this.GetMySqlReaderMethod(nameof(MySqlDataReader.GetUInt32));
            else if (columnInfo.Column.Type == typeof(ulong))
                return this.GetMySqlReaderMethod(nameof(MySqlDataReader.GetUInt64));

            return fallback?.Read(columnInfo);
        }

        public IBindingParameterContract GetParameterContract(IBindingMetadata metadata) => null;
        public IBindingCompositionContract GetCompositionContract(IBindingMetadata metadata) => null;
        public IBindingHelperContract GetHelperContract(IBindingMetadata metadata) => null;

        public IBindingValueContract GetValueContract(IBindingMetadata metadata)
        {
            IBindingValueContract fallback = metadata.Value;

            return new BindingValueContract()
            {
                Convert = fallback.Convert,
                Read = ci => this.GetValueReaderProxy(ci, fallback),
            };
        }

        
    }
}
