using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools.Vendors.Oracle
{
    public class OracleCommandFactory : CommandFactoryBase
    {
        public override InfoCommand GetInfoCommand() => new OracleInfoCommand();
        public override ScaffoldCommand GetScaffoldCommand() => new OracleScaffoldCommand();
    }
}
