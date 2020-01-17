using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class RazorComment : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            while (!tokenizer.Eof && (tokenizer[0] != '*' || tokenizer[1] != '@'))
                tokenizer.Move();

            return true;
        }
    }
}