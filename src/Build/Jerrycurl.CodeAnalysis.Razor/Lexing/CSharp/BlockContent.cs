using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class BlockContent : ISymbol
    {
        public Enclosing Enclosing { get; }

        public BlockContent(Enclosing enclosing)
        {
            this.Enclosing = enclosing;
        }

        public bool Parse(Tokenizer tokenizer)
        {
            int balance = 1;

            while (true)
            {
                tokenizer.Sym(new Literal());
                tokenizer.Sym(new Comment());

                if (tokenizer.Is(this.Enclosing.Start))
                    balance++;
                else if (tokenizer.Is(this.Enclosing.End))
                    balance--;

                if (tokenizer.Eof || balance == 0)
                    break;

                tokenizer.Move();
            }

            return tokenizer.Length > 0;
        }
    }
}
