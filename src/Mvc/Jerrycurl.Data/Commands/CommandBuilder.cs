using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Commands
{
    public class CommandBuilder
    {
        public void Add(IDataReader dataReader)
        {

        }

        public Task AddAsync(DbDataReader dataReader, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Commit()
        {
            // bind to fields!!!
        }
    }
}
