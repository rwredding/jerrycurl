using Jerrycurl.Mvc;
using Jerrycurl.Test;
using Microsoft.Data.Sqlite;

namespace Jerrycurl.Vendors.Sqlite.Test
{
    public class SqliteConvention : DatabaseConvention
    {
        public override void Configure(DomainOptions options)
        {
            options.UseSqlite(GetConnectionString());
        }

        public static string GetConnectionString() => "DATA SOURCE=jerry_test.db";
        public static SqliteConnection GetConnection() => new SqliteConnection(GetConnectionString());
    }
}
