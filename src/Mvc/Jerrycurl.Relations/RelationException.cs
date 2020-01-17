using Jerrycurl.Reflection;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Jerrycurl.Relations
{
    [Serializable]
    public class RelationException : Exception
    {
        public RelationException()
        {

        }

        public RelationException(string message)
            : base(message)
        {

        }

        public RelationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected RelationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region " Exception helpers "

        private static string GetAttributeName(MetadataIdentity identity)
        {
            if (identity == null)
                return "<missing>";
            else if (identity.Schema.Notation.Comparer.Equals(identity.Name, identity.Schema.Notation.Model()))
                return "<model>";
            else
                return identity.Name;
        }

        public static RelationException FromRelation(Type relationType, IReadOnlyList<MetadataIdentity> heading, string message = null, Exception innerException = null)
        {
            string attributeList = string.Join(", ", heading.Select(GetAttributeName));
            string fullMessage = $"An error occurred in relation of type {relationType.GetSanitizedFullName()}({attributeList}).";

            if (message != null || innerException != null)
                fullMessage += $" {message ?? innerException.Message}";

            return new RelationException(fullMessage, innerException);
        }

        public static RelationException FromRelation(RelationIdentity relation, string message = null, Exception innerException = null) => FromRelation(relation.Schema.Model, relation.Heading, message, innerException);
        #endregion
    }
}
