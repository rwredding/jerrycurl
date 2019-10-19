using Jerrycurl.CodeAnalysis.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Enclosing
    {
        public Chars Start { get; }
        public Chars End { get; }

        public Enclosing(string start, string end)
        {
            this.Start = new Chars(start);
            this.End = new Chars(end);
        }
    }
}
