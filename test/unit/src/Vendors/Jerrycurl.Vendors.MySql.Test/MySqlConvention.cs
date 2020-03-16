using Jerrycurl.Mvc;
using Jerrycurl.Test;
using MySql.Data.MySqlClient;

namespace Jerrycurl.Vendors.MySql.Test
{
    public class MySqlConvention : DatabaseConvention
    {
        public override bool Skip => string.IsNullOrEmpty(GetConnectionString());
        public override string SkipReason => "Please configure connection in the 'JERRY_MYSQL_CONN' environment variable.";

        public override void Configure(DomainOptions options)
        {
            options.UseMySql(GetConnectionString());
            options.UseNewtonsoftJson();
        }

        public static string GetConnectionString() => GetEnvironmentVariable("JERRY_MYSQL_CONN");
        public static MySqlConnection GetConnection() => new MySqlConnection(GetConnectionString());
    }
}
