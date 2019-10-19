using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public class AdoConnectionContext
    {
        public IDbConnection Connection { get; }
        public Exception Exception { get; }

        public AdoConnectionContext(IDbConnection connection, Exception exception)
        {
            this.Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.Exception = exception;
        }
    }
}
