using Jerrycurl.Reflection;
using System;
using System.Runtime.Serialization;

namespace Jerrycurl.Relations.Metadata
{
    [Serializable]
    public class MetadataNotFoundException : Exception
    {
        public MetadataNotFoundException()
        {

        }

        public MetadataNotFoundException(string message)
            : base(message)
        {

        }

        public MetadataNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected MetadataNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public static MetadataNotFoundException FromMetadata<TMetadata>(IMetadata metadata, string message = null, Exception innerException = null)
            where TMetadata : IMetadata
            => FromMetadata<TMetadata>(metadata?.Identity, message, innerException);

        public static MetadataNotFoundException FromMetadata<TMetadata>(MetadataIdentity identity, string message = null, Exception innerException = null)
            where TMetadata : IMetadata
        {
            string fullMessage;

            if (identity != null)
                fullMessage = $"Metadata of type '{typeof(TMetadata).GetSanitizedName()}' not found for property '{identity.Name}' in schema '{identity.Schema.Model.GetSanitizedFullName()}'.";
            else
                fullMessage = $"Metadata of type '{typeof(TMetadata).GetSanitizedName()}' not found.";

            if (message != null || innerException?.Message != null)
                fullMessage += $" {message}";

            return new MetadataNotFoundException(fullMessage, innerException);
        }

    }
}
