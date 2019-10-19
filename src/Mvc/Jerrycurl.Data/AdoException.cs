using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace Jerrycurl.Data
{
    [Serializable]
    public class AdoException : Exception
    {
        public IDbCommand Command { get; internal set; }
        public IDbConnection Connection { get; internal set; }

        public AdoException()
        {

        }

        public AdoException(string message)
            : base(message)
        {

        }

        public AdoException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected AdoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
