using System;
using System.Collections.Generic;
using System.Data;

namespace Jerrycurl.Data.Sessions
{
    public interface ISyncSession : IDisposable
    {
        IEnumerable<IDataReader> Execute(IOperation operation);
    }
}
