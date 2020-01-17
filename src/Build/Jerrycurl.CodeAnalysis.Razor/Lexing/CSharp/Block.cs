using System;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Block : ISymbol
    {
        public Enclosing Enclosing { get; }

        public Block(Enclosing enclosing)
        {
            this.Enclosing = enclosing ?? throw new ArgumentNullException(nameof(enclosing));
        }

        public bool Parse(Tokenizer tokenizer)
        {
            if (!tokenizer.Sym(this.Enclosing.Start))
                return false;

            tokenizer.Sym(new BlockContent(this.Enclosing));
            tokenizer.Sym(this.Enclosing.End);

            return true;
        }
    }
}
