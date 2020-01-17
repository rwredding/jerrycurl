using Jerrycurl.Data.Metadata;
using Jerrycurl.Reflection;
using System;
using System.Runtime.Serialization;

namespace Jerrycurl.Data
{
    [Serializable]
    public class BindingException : Exception
    {
        public BindingException()
        {

        }

        public BindingException(string message)
            : base(message)
        {

        }

        public BindingException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected BindingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region " Exception helpers "

        public static BindingException FromReference(Type schemaType, string propertyName, string foreignPropertyName, string message = null, Exception innerException = null)
        {
            string fullMessage = $"Unable to bind join between '{propertyName}' and '{foreignPropertyName}' in model '{schemaType.GetSanitizedFullName()}'.";

            if (message != null || innerException?.Message != null)
                fullMessage += $" {message ?? innerException?.Message}";

            return new BindingException(fullMessage, innerException);
        }

        public static BindingException FromProperty(Type schemaType, string propertyName, string message = null, Exception innerException = null)
        {
            message = message ?? innerException?.Message;

            if (schemaType != null && message != null)
                message = $"Unable to bind to property '{propertyName}' in model '{schemaType.GetSanitizedFullName()}'. {message}";
            else if (schemaType != null && message == null)
                message = $"Unable to bind to property '{propertyName}' in model '{schemaType.GetSanitizedFullName()}'.";
            else if (message != null)
                message = $"Unable to bind to property '{propertyName}'. {message}";
            else
                message = $"Unable to bind property '{propertyName}'.";

            return new BindingException(message, innerException);
        }

        public static BindingException FromMetadata(IBindingMetadata metadata, string message = null, Exception innerException = null) => FromProperty(metadata.Identity.Schema.Model, metadata.Identity.Name, message, innerException);
        public static BindingException FromProperty(string propertyName, string message = null, Exception innerException = null) => FromProperty(null, propertyName, message, innerException);

        #endregion
    }
}
