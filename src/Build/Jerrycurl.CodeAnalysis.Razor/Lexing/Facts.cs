using System.Collections.Generic;
using System.Globalization;
using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing
{
    public static class Facts
    {
        public static Enclosing Indexer { get; } = new Enclosing("[", "]");
        public static Enclosing Statement { get; } = new Enclosing("{", "}");
        public static Enclosing Expression { get; } = new Enclosing("(", ")");
        public static Enclosing Generic { get; } = new Enclosing("<", ">");

        public static Enclosing CharLiteral { get; } = new Enclosing("'", "'");
        public static Enclosing Literal { get; } = new Enclosing("\"", "\"");
        public static Enclosing VerbatimLiteral { get; } = new Enclosing("@\"", "\"");
        public static Enclosing InterpolatedLiteral { get; } = new Enclosing("$\"", "\"");
        public static Enclosing InterpolatedVerbatimLiteral { get; } = new Enclosing("$@\"", "\"");

        public static char Qualifier { get; } = '.';

        public static IEnumerable<string> Keywords { get; } = new[] { "if", "else", "foreach", "for", "while", "using" };
        public static IEnumerable<string> Reserved { get; } = new[] { "{", "}" };

        public static bool IsIdentifier(this Tokenizer tokenizer)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(tokenizer[0] ?? '\0');

            switch (category)
            {
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.Format:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    tokenizer.Move();
                    return true;
                default:
                    return false;
            }
        }
    }
}
