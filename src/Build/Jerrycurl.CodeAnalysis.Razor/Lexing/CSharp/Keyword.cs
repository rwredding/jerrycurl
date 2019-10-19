using Jerrycurl.CodeAnalysis.Lexing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Keyword : ISymbol
    {
        public bool Parse(Tokenizer tokenizer) => Facts.Keywords.Any(kw => tokenizer.Func(t => this.ParseKeyword(t, kw)));

        private bool ParseKeyword(Tokenizer tokenizer, string keyword)
        {
            if (tokenizer.String(keyword) && !tokenizer.IsIdentifier())
                return true;

            return false;
        }
    }
}