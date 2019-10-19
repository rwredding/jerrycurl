using Jerrycurl.Diagnostics;
using Jerrycurl.Reflection;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Jerrycurl.Relations.Metadata
{
    [Serializable]
    public class MetadataBuilderException : Exception
    {
        public MetadataBuilderException()
        {

        }

        public MetadataBuilderException(string message)
            : base(message)
        {

        }

        public MetadataBuilderException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected MetadataBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
