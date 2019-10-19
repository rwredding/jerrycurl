using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class BlockStart : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => tokenizer.Sym(Facts.Statement.Start);
    }
}
