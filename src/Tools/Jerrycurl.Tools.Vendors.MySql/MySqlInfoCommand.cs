using Jerrycurl.Tools.Info;
using MySql.Data.MySqlClient;

namespace Jerrycurl.Tools.Vendors.MySql
{
    public class MySqlInfoCommand : InfoCommand
    {
        public override string Vendor => "MySQL";
        public override string Connector => typeof(MySqlConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(MySqlConnection).Assembly.GetName().Version.ToString();
    }
}
