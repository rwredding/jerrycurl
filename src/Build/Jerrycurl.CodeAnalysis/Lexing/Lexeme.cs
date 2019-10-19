using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public class Lexeme
    {
        public IRule Rule { get; set; }
        public SourceSpan Span { get; set; }
        public IEnumerable<Token> Tokens { get; set; }
        public IEnumerable<Lexeme> Lexemes { get; set; }

        public override string ToString() => $"{this.Span}: {this.Rule}";
    }
}
