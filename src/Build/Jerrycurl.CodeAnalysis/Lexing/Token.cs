using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public class Token
    {
        public ISymbol Symbol { get; }
        public SourceSpan Span { get; }

        public Token(ISymbol symbol, SourceSpan span)
        {
            this.Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            this.Span = span;
        }

        public override string ToString() => $"{this.Span}: {this.Symbol}";
    }
}
