using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools.Vendors.MySql
{
    public class MySqlCommandFactory : CommandFactoryBase
    {
        public override InfoCommand GetInfoCommand() => new MySqlInfoCommand();
        public override ScaffoldCommand GetScaffoldCommand() => new MySqlScaffoldCommand();
    }
}
