using System;
using System.Data;

namespace Jerrycurl.Data.Filters
{
    public class FilterContext
    {
        public object SourceObject { get; }
        public IDbConnection Connection { get; }
        public IDbCommand Command { get; }
        public Exception Exception { get; }

        internal FilterContext(IDbConnection connection, Exception exception, object sourceObject = null)
        {
            this.Connection = connection;
            this.Exception = exception;
            this.SourceObject = sourceObject;
        }

        internal FilterContext(IDbCommand command, Exception exception, object sourceObject = null)
        {
            this.Command = command;
            this.Connection = command?.Connection;
            this.Exception = exception;
            this.SourceObject = sourceObject;
        }
    }
}
