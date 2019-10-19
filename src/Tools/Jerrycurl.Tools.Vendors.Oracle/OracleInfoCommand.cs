using Jerrycurl.Tools.Info;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Tools.Vendors.Oracle
{
    public class OracleInfoCommand : InfoCommand
    {
        public override string Vendor => "Oracle";
        public override string Connector => typeof(OracleConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(OracleConnection).Assembly.GetName().Version.ToString();
    }
}
