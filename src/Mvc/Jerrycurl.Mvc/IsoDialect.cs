using System;
using System.Globalization;
using System.Linq;

namespace Jerrycurl.Mvc
{
    public class IsoDialect : IDialect
    {
        protected virtual char IdentifierQuote { get; } = '"';
        protected virtual char? VariablePrefix { get; } = '@';
        protected virtual char? ParameterPrefix { get; } = '@';
        protected virtual char? StringPrefix { get; } = null;

        public virtual string Qualifier => ".";

        public virtual string Literal(object value)
        {
            switch (value)
            {
                case null:
                    return "NULL";
                case int _:
                case long _:
                case short _:
                case uint _:
                case ushort _:
                case byte _:
                case sbyte _:
                case ulong _:
                case float _:
                case double _:
                case decimal _:
                    return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
            }

            return null;
        }

        public virtual string String(string value)
        {
            if (value.Contains('\''))
                throw new InvalidOperationException("String literal cannot contain the ' character.");

            return value != null ? $"{this.StringPrefix}'{value}'" : "NULL";
        }

        public virtual string Identifier(string identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));
            else if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be blank.", nameof(identifier));
            else if (identifier.Contains(this.IdentifierQuote))
                throw new ArgumentException($"Identifier cannot contain the {this.IdentifierQuote} character.", nameof(identifier));

            return this.IdentifierQuote + identifier + this.IdentifierQuote;
        }

        public virtual string Parameter(string parameterName)
        {
            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));
            else if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentException("Parameter name cannot be blank.", nameof(parameterName));

            if (this.ParameterPrefix == null || parameterName[0] == this.ParameterPrefix)
                return parameterName;

            return this.ParameterPrefix + parameterName;
        }

        public virtual string Variable(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException(nameof(variableName));
            else if (string.IsNullOrWhiteSpace(variableName))
                throw new ArgumentException("Variable name cannot be blank.", nameof(variableName));

            if (this.VariablePrefix == null || variableName[0] == this.VariablePrefix)
                return variableName;

            return this.VariablePrefix + variableName;
        }
    }
}
