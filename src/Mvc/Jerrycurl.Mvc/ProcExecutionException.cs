using System;
using System.Runtime.Serialization;

namespace Jerrycurl.Mvc
{
    public class ProcExecutionException : Exception
    {
        public ProcExecutionException()
        {

        }

        public ProcExecutionException(string message)
            : base(message)
        {

        }

        public ProcExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected ProcExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
