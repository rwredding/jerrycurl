using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public static class LexerExtensions
    {
        public static bool Many(this Lexer lexer, Func<Lexer, bool> predicate)
        {
            if (lexer.Eof)
                return false;
            else if (!predicate(lexer))
                return false;

            bool result = true;

            while (result && !lexer.Eof)
                result = predicate(lexer);

            return true;
        }

        public static bool Yield(this Lexer lexer, ISymbol symbol) => lexer.Yield(new SymbolRule(symbol));
        public static bool Many(this Lexer lexer, ISymbol symbol) => lexer.Many(l => l.Yield(symbol));
    }
}
