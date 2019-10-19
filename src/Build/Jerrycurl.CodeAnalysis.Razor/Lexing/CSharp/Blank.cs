using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Blank : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => tokenizer.Blank();
    }
}
