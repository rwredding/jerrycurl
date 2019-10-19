using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Tools
{
    public interface ICommandFactory
    {
        InfoCommand GetInfoCommand();
        ScaffoldCommand GetScaffoldCommand();
    }
}
