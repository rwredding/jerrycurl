using System;
using System.Runtime.Serialization;

namespace Jerrycurl.Mvc
{
    public class PageNotFoundException : Exception
    {
        public PageNotFoundException()
        {

        }

        public PageNotFoundException(string message)
            : base(message)
        {

        }

        public PageNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected PageNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
