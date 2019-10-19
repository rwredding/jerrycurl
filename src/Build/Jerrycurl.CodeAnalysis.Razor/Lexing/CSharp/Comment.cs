using Jerrycurl.CodeAnalysis.Lexing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Comment : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            if (tokenizer.String("//"))
            {
                while (!tokenizer.Eof && !tokenizer.IsBreak())
                    tokenizer.Move();

                return true;
            }
            else if (tokenizer.String("/*"))
            {
                while (!tokenizer.Eof && tokenizer[0] != '*' || tokenizer[1] != '/')
                    tokenizer.Move();

                if (!tokenizer.Eof)
                    tokenizer.Move(2);

                return true;
            }

            return false;
        }
    }
}