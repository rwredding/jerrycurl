using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Lexing.Razor;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing
{
    public class RazorBlock : IRule
    {
        public RazorType Type { get; private set; }

        public bool Parse(Lexer lexer)
        {
            RazorStartTag startTag = new RazorStartTag();

            if (lexer.Yield(startTag))
            {
                this.Type = startTag.Type;

                lexer.Yield(new CSharpBlock(this.Type));
                lexer.Yield(new RazorEndTag(startTag.Type));

                return true;
            }

            return false;
        }
    }
}
