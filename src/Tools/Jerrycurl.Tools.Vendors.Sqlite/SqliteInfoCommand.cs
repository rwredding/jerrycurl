using Jerrycurl.Tools.Info;
using Microsoft.Data.Sqlite;

namespace Jerrycurl.Tools.Vendors.Sqlite
{
    public class SqliteInfoCommand : InfoCommand
    {
        public override string Vendor => "SQLite";
        public override string Connector => typeof(SqliteConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(SqliteConnection).Assembly.GetName().Version.ToString();
    }
}
