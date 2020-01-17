using Jerrycurl.Mvc;
using Jerrycurl.Test;

namespace Jerrycurl.Vendors.Sqlite.Test
{
    public class SqliteConvention : DatabaseConvention
    {
        public override void Configure(DomainOptions options)
        {
            options.UseSqlite("DATA SOURCE=jerry_test.db");
        }
    }
}
