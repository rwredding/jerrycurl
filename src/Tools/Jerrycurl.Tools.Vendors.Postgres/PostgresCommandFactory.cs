using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools.Vendors.Postgres
{
    public class PostgresCommandFactory : CommandFactoryBase
    {
        public override InfoCommand GetInfoCommand() => new PostgresInfoCommand();
        public override ScaffoldCommand GetScaffoldCommand() => new PostgresScaffoldCommand();
    }
}
