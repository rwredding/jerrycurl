﻿using Jerrycurl.Tools.Info;
using Microsoft.Data.SqlClient;

namespace Jerrycurl.Tools.Vendors.SqlServer
{
    public class SqlServerInfoCommand : InfoCommand
    {
        public override string Vendor => "Microsoft SQL Server";
        public override string Connector => typeof(SqlConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(SqlConnection).Assembly.GetName().Version.ToString();
    }
}
