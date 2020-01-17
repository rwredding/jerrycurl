using System.Data.Common;

namespace Jerrycurl.Tools
{
    public interface IConnectionFactory
    {
        DbConnection GetDbConnection();
    }
}
