using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class BlockStart : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => tokenizer.Sym(Facts.Statement.Start);
    }
}
