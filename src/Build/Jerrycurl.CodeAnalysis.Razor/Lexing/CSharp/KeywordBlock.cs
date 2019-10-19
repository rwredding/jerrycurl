using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class KeywordBlock : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            if (!this.ParseKeyword(tokenizer))
                return false;

            tokenizer.Blanks();

            this.ParseKeyword(tokenizer);

            tokenizer.Blanks();

            if (tokenizer.Sym(new Block(Facts.Expression)))
            {
                tokenizer.Blanks();

                if (!tokenizer.Is(Facts.Statement.Start) && tokenizer[0] != '@')
                    tokenizer.Sym(new Argument(ArgumentType.Word, false));
            }
            else if (tokenizer[0] != '@')
                tokenizer.Sym(new Argument(ArgumentType.Word, false));

            return true;
        }

        private bool ParseKeyword(Tokenizer tokenizer) => Facts.Keywords.Any(kw => tokenizer.Func(t => this.ParseKeyword(t, kw)));

        private bool ParseKeyword(Tokenizer tokenizer, string keyword)
        {
            if (tokenizer.String(keyword) && !tokenizer.IsIdentifier())
                return true;

            return false;
        }
    }
}
