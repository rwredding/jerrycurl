using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Lexing.Sql;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing
{
    public class SqlBlock : IRule
    {
        public bool Parse(Lexer lexer) => lexer.Many(this.ParseSql);

        private bool ParseSql(Lexer lexer)
        {
            if (lexer.Yield(new SqlCode()))
                return true;
            else if (lexer.Yield(new SqlEscape()))
                return true;

            return false;
        }
    }
}
