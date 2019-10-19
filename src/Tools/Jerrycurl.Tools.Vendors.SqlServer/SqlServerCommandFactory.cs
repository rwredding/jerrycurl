using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools.Vendors.SqlServer
{
    public class SqlServerCommandFactory : CommandFactoryBase
    {
        public override InfoCommand GetInfoCommand() => new SqlServerInfoCommand();
        public override ScaffoldCommand GetScaffoldCommand() => new SqlServerScaffoldCommand();
    }
}
