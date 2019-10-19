using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Filters
{
    public class AdoCommandContext
    {
        public IDbCommand Command { get; }
        public Exception Exception { get; }

        public AdoCommandContext(IDbCommand command, Exception exception)
        {
            this.Command = command ?? throw new ArgumentNullException(nameof(command));
            this.Exception = exception;
        }
    }
}
