using Jerrycurl.Mvc;
using Jerrycurl.Test;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Vendors.Oracle.Test
{
    public class OracleConvention : DatabaseConvention
    {
        public override bool Skip => string.IsNullOrEmpty(GetConnectionString());
        public override string SkipReason => "Please configure connection in the 'JERRY_ORACLE_CONN' environment variable.";

        public override void Configure(DomainOptions options)
        {
            options.UseOracle(GetConnectionString());
            options.UseNewtonsoftJson();
        }

        public static string GetConnectionString() => GetEnvironmentVariable("JERRY_ORACLE_CONN");
        public static OracleConnection GetConnection() => new OracleConnection(GetConnectionString());
    }
}
