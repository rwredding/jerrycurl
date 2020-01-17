using Jerrycurl.CodeAnalysis.Lexing;
using System;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing
{
    public class RazorDocument : IRule
    {
        public bool Parse(Lexer lexer)
        {
            while (!lexer.Eof)
            {
                bool flag1 = lexer.Yield(new SqlBlock());
                bool flag2 = lexer.Yield(new RazorBlock());

                if (!flag1 && !flag2)
                    throw new InvalidOperationException("Lexing stalled.");
            }

            return true;
        }
    }
}
