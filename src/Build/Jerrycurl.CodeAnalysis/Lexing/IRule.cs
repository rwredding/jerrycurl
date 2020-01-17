namespace Jerrycurl.CodeAnalysis.Lexing
{
    public interface IRule
    {
        bool Parse(Lexer lexer);
    }
}
