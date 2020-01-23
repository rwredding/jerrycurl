using Jerrycurl.Mvc;
using Jerrycurl.Test;
using Microsoft.Data.SqlClient;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class SqlServerConvention : DatabaseConvention
    {
        public override bool Skip => string.IsNullOrEmpty(GetConnectionString());
        public override string SkipReason => "Please configure connection in the 'JERRY_SQLSERVER_CONN' environment variable.";

        public override void Configure(DomainOptions options)
        {
            options.UseSqlServer(GetConnectionString());
            options.UseNewtonsoftJson();
        }

        public static string GetConnectionString() => GetEnvironmentVariable("JERRY_SQLSERVER_CONN");
        public static SqlConnection GetConnection() => new SqlConnection(GetConnectionString());
    }
}
