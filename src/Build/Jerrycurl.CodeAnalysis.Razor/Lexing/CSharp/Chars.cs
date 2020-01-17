using Jerrycurl.CodeAnalysis.Lexing;
using System;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Chars : ISymbol
    {
        public string Value { get; }

        public Chars(string value)
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Parse(Tokenizer tokenizer) => tokenizer.String(this.Value);
    }
}
