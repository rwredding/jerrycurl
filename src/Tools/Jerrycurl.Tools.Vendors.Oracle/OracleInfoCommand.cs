using Jerrycurl.Tools.Info;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Tools.Vendors.Oracle
{
    public class OracleInfoCommand : InfoCommand
    {
        public override string Vendor => "Oracle";
        public override string Connector => typeof(OracleConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(OracleConnection).Assembly.GetName().Version.ToString();
    }
}
