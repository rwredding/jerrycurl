using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;

namespace Jerrycurl.Data.Sessions
{
    public partial interface IAsyncSession : IAsyncDisposable
    {
        IAsyncEnumerable<DbDataReader> ExecuteAsync(IOperation operation, CancellationToken cancellationToken);
    }
}
