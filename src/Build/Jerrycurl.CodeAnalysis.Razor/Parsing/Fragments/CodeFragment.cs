using Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing.Fragments
{
    public class CodeFragment : RazorFragment
    {
        public CSharpType CodeType { get; set; }
    }
}
