using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Sessions
{
    public partial interface IAsyncSession : IDisposable
    {
        IAsyncEnumerable<DbDataReader> ExecuteAsync(IOperation operation, CancellationToken cancellationToken);
    }
}
