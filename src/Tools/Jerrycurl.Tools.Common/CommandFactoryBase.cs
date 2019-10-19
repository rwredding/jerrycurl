using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools
{
    public class CommandFactoryBase : ICommandFactory
    {
        public virtual ScaffoldCommand GetScaffoldCommand() => null;
        public virtual InfoCommand GetInfoCommand() => null;
    }
}
