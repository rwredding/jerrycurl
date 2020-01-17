namespace Jerrycurl.CodeAnalysis.Lexing
{
    public interface ISymbol
    {
        bool Parse(Tokenizer tokenizer);
    }
}