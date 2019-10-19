using Jerrycurl.CodeAnalysis.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.Razor
{
    public class RazorEndTag : ISymbol
    {
        public RazorType Tag { get; private set; }

        public RazorEndTag(RazorType startTag)
        {
            this.Tag = startTag;
        }

        public bool Parse(Tokenizer tokenizer)
        {
            if (this.Tag == RazorType.Expression)
                return tokenizer.Char(')');
            else if (this.Tag == RazorType.Statement)
                return tokenizer.Char('}');
            else if (this.Tag == RazorType.Comment)
                return tokenizer.String("*@");

            return true;
        }
    }
}
