using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Vendors.Oracle
{
    public class OracleContractResolver : IBindingContractResolver
    {
        public int Priority => 1000;

        private MethodInfo GetOracleReaderMethod(string methodName)
        {
            Type reader = typeof(OracleDataReader);

            return reader.GetMethod(methodName, new[] { typeof(int) }) ?? throw new InvalidOperationException("No such method.");
        }

        private MethodInfo GetValueReaderProxy(IBindingColumnInfo columnInfo, IBindingValueContract fallback)
        {
            if (columnInfo.Column.Type == typeof(TimeSpan))
                return this.GetOracleReaderMethod(nameof(OracleDataReader.GetTimeSpan));
            else if (columnInfo.Column.Type == typeof(DateTimeOffset) || columnInfo.Column.TypeName == "TimeStampTZ")
                return this.GetOracleReaderMethod(nameof(OracleDataReader.GetDateTimeOffset));

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
