using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public interface ICommandBinding
    {
        IField Target { get; }
    }
}
