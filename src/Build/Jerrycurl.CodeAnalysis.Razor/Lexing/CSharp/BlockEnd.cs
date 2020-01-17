using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class BlockEnd : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            if (!tokenizer.Sym(Facts.Statement.End))
                return false;

            tokenizer.Blanks();

            return true;
        }
    }
}
