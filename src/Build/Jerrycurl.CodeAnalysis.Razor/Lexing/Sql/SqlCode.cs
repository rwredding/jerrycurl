using Jerrycurl.CodeAnalysis.Lexing;
using System.Linq;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.Sql
{
    public class SqlCode : ISymbol
    {
        public static char[] Illegal = new[] { '{', '}', '@' };

        public bool Parse(Tokenizer tokenizer)
        {
            return tokenizer.Many(this.ParseChars);
        }

        private bool ParseChars(Tokenizer tokenizer)
        {
            if (tokenizer.Eof)
                return false;
            else if (Illegal.Any(c => tokenizer[0] == c))
                return false;

            tokenizer.Move();

            return true;
        }
    }
}