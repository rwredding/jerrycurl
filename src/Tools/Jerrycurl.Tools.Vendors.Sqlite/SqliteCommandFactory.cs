using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools.Vendors.Sqlite
{
    public class SqliteCommandFactory : CommandFactoryBase
    {
        public override InfoCommand GetInfoCommand() => new SqliteInfoCommand();
        public override ScaffoldCommand GetScaffoldCommand() => new SqliteScaffoldCommand();
    }
}
