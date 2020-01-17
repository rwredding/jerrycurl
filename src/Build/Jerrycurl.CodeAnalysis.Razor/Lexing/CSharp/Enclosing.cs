namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Enclosing
    {
        public Chars Start { get; }
        public Chars End { get; }

        public Enclosing(string start, string end)
        {
            this.Start = new Chars(start);
            this.End = new Chars(end);
        }
    }
}
