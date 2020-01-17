using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Identifier : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => tokenizer.Many(Facts.IsIdentifier);
    }
}