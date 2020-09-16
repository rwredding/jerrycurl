using System;
using System.Data;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Filters
{
    public class FilterContext
    {
        public IBatch Batch { get; }
        public IDbConnection Connection { get; }
        public IDbCommand Command { get; }
        public Exception Exception { get; }
        public bool IsHandled { get; set; }

        internal FilterContext(IDbConnection connection, Exception exception, IBatch batch = null)
        {
            this.Connection = connection;
            this.Exception = exception;
            this.Batch = batch;
        }

        internal FilterContext(IDbConnection connection, IDbCommand command, Exception exception, IBatch batch = null)
        {
            this.Connection = connection;
            this.Command = command;
            this.Exception = exception;
            this.Batch = batch;
        }
    }
}
