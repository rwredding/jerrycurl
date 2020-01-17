using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Projection.Internal
{
    internal class Chunks : IRule
    {
        public bool Parse(Lexer lexer) => lexer.Many(this.ParseInertOrCode);

        private bool ParseInertOrCode(Lexer lexer)
        {
            if (lexer.Yield(new Inert()))
                return true;
            else if (lexer.Yield(new Code()))
                return true;

            return false;
        }
    }
}
