using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorFragment
    {
        public SourceSpan? Span { get; set; }
        public string Text { get; set; }
        public string SourceName { get; set; }

        public override string ToString() => $"{this.GetType().Name} {this.Span}: '{this.Text}'";
    }
}
