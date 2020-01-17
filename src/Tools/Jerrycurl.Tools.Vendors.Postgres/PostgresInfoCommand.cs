using Jerrycurl.Tools.Info;
using Npgsql;

namespace Jerrycurl.Tools.Vendors.Postgres
{
    public class PostgresInfoCommand : InfoCommand
    {
        public override string Vendor => "PostgreSQL";
        public override string Connector => typeof(NpgsqlConnection).Assembly.GetName().Name;
        public override string ConnectorVersion => typeof(NpgsqlConnection).Assembly.GetName().Version.ToString();
    }
}
