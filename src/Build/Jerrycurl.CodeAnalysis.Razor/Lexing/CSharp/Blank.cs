using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Blank : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => tokenizer.Blank();
    }
}
