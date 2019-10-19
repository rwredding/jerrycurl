using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jerrycurl.CodeAnalysis
{
    internal static class CSharp
    {
        public static string Identifier(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Identifier cannot be empty.", nameof(s));

            string identifier = new string(s.Select(c => IsIdentifierPart(c) ? c : '_').ToArray());

            if (!IsIdentifierStart(s[0]))
                identifier = $"_{identifier}";

            return identifier;
        }

        public static string Literal(string s)
        {
            if (s == null)
                return "null";

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                char c0 = s[i];

                switch (c0)
                {
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\0':
                        builder.Append("\\0");
                        break;
                    case '\a':
                        builder.Append("\\a");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\v':
                        builder.Append("\\v");
                        break;
                    case '\"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\");
                        break;
                    default:
                        builder.Append(c0);
                        break;
                }
            }

            return $"\"{builder.ToString()}\"";
        }


        private static bool IsIdentifierStart(char c)
        {
            return (char.IsLetter(c) || c == '_' || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber);
        }

        private static bool IsIdentifierPart(char c)
        {
            if (char.IsDigit(c))
                return true;
            else if (IsIdentifierStart(c))
                return true;
            else
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.Format:
                        return true;
                }
            }

            return false;
        }
    }
}
