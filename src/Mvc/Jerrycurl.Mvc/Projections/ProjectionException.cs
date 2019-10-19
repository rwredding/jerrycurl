using Jerrycurl.Reflection;
using System;
using System.Runtime.Serialization;

namespace Jerrycurl.Mvc.Projections
{
    [Serializable]
    public class ProjectionException : Exception
    {
        public ProjectionException()
        {

        }

        public ProjectionException(string message)
            : base(message)
        {

        }

        public ProjectionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected ProjectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region " Exception helpers "

        public static ProjectionException FromAttribute(Type schemaType, string attributeName, string message = null, Exception innerException = null)
        {
            message = message ?? innerException?.Message;

            if (schemaType == null && attributeName == null && message != null)
                message = $"Unable to create projection. {message}";
            else if (schemaType == null && attributeName == null)
                message = $"Unable to create projection.";
            else if (schemaType != null && message != null)
                message = $"Unable to create projection from attribute '{attributeName}' in schema '{schemaType.GetSanitizedFullName()}'. {message}";
            else if (schemaType != null)
                message = $"Unable to create projection from attribute '{attributeName}' in schema '{schemaType.GetSanitizedFullName()}'.";
            else if (message != null)
                message = $"Unable to create projection from attribute '{attributeName}'. {message}";
            else
                message = $"Unable to create projection from attribute '{attributeName}'.";

            return new ProjectionException(message, innerException);
        }

        public static ProjectionException FromProjection(IProjection projection, string message = null, Exception innerException = null) => FromAttribute(projection.Metadata?.Identity.Schema.Model, projection.Metadata?.Identity.Name, message, innerException);
        public static ProjectionException FromProjection(IProjectionAttribute attribute, string message = null, Exception innerException = null) => FromAttribute(attribute.Metadata?.Identity.Schema.Model, attribute.Metadata?.Identity.Name, message, innerException);
        public static ProjectionException FromAttribute(string attributeName, string message = null, Exception innerException = null) => FromAttribute(null, attributeName, message, innerException);

        public static ProjectionException ArgumentNull(string argumentName, IProjection projection = null) => FromProjection(projection, innerException: new ArgumentNullException(argumentName));
        public static ProjectionException ArgumentNull(string argumentName, IProjectionAttribute attribute) => FromProjection(attribute, innerException: new ArgumentNullException(argumentName));

        public static ProjectionException ValueNotFound(IProjection projection) => FromProjection(projection, "Value not found.");
        public static ProjectionException ValueNotFound(IProjectionAttribute attribute) => FromProjection(attribute, "Value not found.");

        #endregion
    }
}
