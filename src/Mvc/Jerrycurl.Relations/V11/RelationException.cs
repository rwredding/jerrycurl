using Jerrycurl.Reflection;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Jerrycurl.Relations.V11
{
    [Serializable]
    public class RelationException2 : Exception
    {
        public RelationException2()
        {

        }

        public RelationException2(string message)
            : base(message)
        {

        }

        public RelationException2(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected RelationException2(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region " Exception helpers "

        private static string GetAttributeName(RelationAttribute attribute)
        {
            if (attribute == null)
                return "<missing>";
            else if (attribute.Schema.Notation.Comparer.Equals(attribute.Name, attribute.Schema.Notation.Model()))
                return "<model>";
            else
                return attribute.Name;
        }

        public static RelationException2 FromRelation(Type relationType, RelationHeader header, string message = null, Exception innerException = null)
        {
            string attributeList = string.Join(", ", header.Attributes.Select(GetAttributeName));
            string fullMessage = $"An error occurred in relation of type {relationType.GetSanitizedFullName()}({attributeList}).";

            if (message != null || innerException != null)
                fullMessage += $" {message ?? innerException.Message}";

            return new RelationException2(fullMessage, innerException);
        }

        public static RelationException2 FromRelation(RelationHeader header, string message = null, Exception innerException = null) => FromRelation(header.Schema.Model, header, message, innerException);
        #endregion
    }
}
