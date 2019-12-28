using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public class FilterContext
    {
        public IDbConnection Connection { get; }
        public IDbCommand Command { get; }
        public Exception Exception { get; }

        internal FilterContext(IDbConnection connection, Exception exception)
        {
            this.Connection = connection;
            this.Exception = exception;
        }

        internal FilterContext(IDbCommand command, Exception exception)
        {
            this.Command = command;
            this.Connection = command?.Connection;
            this.Exception = exception;
        }
    }
}
