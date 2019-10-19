using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Projection.Internal
{
    internal class Inert : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            while (!tokenizer.Eof && tokenizer[0] != '$')
                tokenizer.Move();

            return (tokenizer.Length > 0);
        }
    }
}
