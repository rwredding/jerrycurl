using Jerrycurl.Tools.Info;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Tools.Vendors.Sqlite
{
    public class SqliteInfoCommand : InfoCommand
    {
        public override string Vendor => "SQLite";
        public override string Connector => typeof(SqliteConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(SqliteConnection).Assembly.GetName().Version.ToString();
    }
}
