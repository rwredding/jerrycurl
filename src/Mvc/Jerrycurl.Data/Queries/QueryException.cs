using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Jerrycurl.Data.Queries
{
    [Serializable]
    public class QueryException : Exception
    {
        public QueryException()
        {

        }

        public QueryException(string message)
            : base(message)
        {

        }

        public QueryException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected QueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
