namespace Jerrycurl.CodeAnalysis.Lexing
{
    public interface ISource
    {
        char? this[int position] { get; }

        bool Eof { get; }
        bool Bof { get; }

        SourceSpan Read(int length);
        void Discard(int length);

        string GetText(SourceSpan span);
        string GetText();
    }
}
