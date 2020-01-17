using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;

namespace Jerrycurl.Tools
{
    public interface ICommandFactory
    {
        InfoCommand GetInfoCommand();
        ScaffoldCommand GetScaffoldCommand();
    }
}
