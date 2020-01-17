using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Inline : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            if (tokenizer.Eof)
                return true;

            if (tokenizer.Sym(new Keyword()) || !tokenizer.Sym(new Identifier()))
                return false;

            tokenizer.Many(this.ParseSuffix);

            return true;
        }

        private bool ParseSuffix(Tokenizer tokenizer)
        {
            if (tokenizer.Char(Facts.Qualifier))
            {
                tokenizer.Sym(new Identifier());

                return true;
            }
            else if (tokenizer.Sym(new Block(Facts.Statement)))
                return true;
            else if (tokenizer.Sym(new Block(Facts.Indexer)))
                return true;
            else if (tokenizer.Sym(new Block(Facts.Expression)))
                return true;
            else if (tokenizer.Sym(new Block(Facts.Generic)))
                return true;

            return false;
        }
    }
}