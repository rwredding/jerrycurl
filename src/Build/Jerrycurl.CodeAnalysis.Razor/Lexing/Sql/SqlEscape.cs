using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.Sql
{
    public class SqlEscape : ISymbol
    {
        public static char[] Illegal = new[] { '{', '}', '@' };

        public char Char { get; private set; }

        public bool Parse(Tokenizer tokenizer) 
        {
            foreach (char c in Illegal)
            {
                if (tokenizer[0] == c && tokenizer[1] == c)
                {
                    this.Char = c;

                    tokenizer.Move(2);

                    return true;
                }
            }

            return false;
        }
    }
}
