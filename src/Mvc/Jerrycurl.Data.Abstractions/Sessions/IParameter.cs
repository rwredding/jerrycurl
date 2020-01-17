using System.Data;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Sessions
{
    public interface IParameter
    {
        string Name { get; }
        IField Field { get; }

        void Build(IDbDataParameter adoParameter);
    }
}
