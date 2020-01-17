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
