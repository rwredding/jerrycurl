using System;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public class SymbolRule : IRule
    {
        public ISymbol Symbol { get; }
        public Token Token { get; private set; }

        public SymbolRule(ISymbol symbol)
        {
            this.Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public bool Parse(Lexer lexer)
        {
            Tokenizer tokenizer = new Tokenizer(lexer.Source);

            if (this.Symbol.Parse(tokenizer))
            {
                this.Token = new Token(this.Symbol, tokenizer.Accept());

                return true;
            }

            return false;
        }

        public override string ToString() => this.Symbol.ToString();
    }
}
