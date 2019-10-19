using Jerrycurl.CodeAnalysis.Lexing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Identifier : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => tokenizer.Many(Facts.IsIdentifier);
    }
}