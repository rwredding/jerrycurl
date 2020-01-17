using System.Data;

namespace Jerrycurl.Data.Sessions
{
    public interface IOperation
    {
        object Source { get; }

        void Build(IDbCommand adoCommand);
    }
}
