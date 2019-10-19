using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Argument : ISymbol
    {
        public ArgumentType Type { get; }
        public bool CanBeEmpty { get; }

        public Argument(ArgumentType argumentType, bool canBeEmpty)
        {
            this.Type = argumentType;
            this.CanBeEmpty = canBeEmpty;
        }

        public bool Parse(Tokenizer tokenizer)
        {
            if (tokenizer.Eof || tokenizer.IsBreak())
                return this.CanBeEmpty;

            switch (this.Type)
            {
                case ArgumentType.Line:
                    {
                        while (!tokenizer.Eof && !tokenizer.IsBreak())
                            tokenizer.Move();
                    }
                    break;
                case ArgumentType.Word:
                    {
                        while (tokenizer.IsBlank() && !tokenizer.IsBreak())
                            tokenizer.Move();

                        while (!tokenizer.Eof && !tokenizer.IsBlank())
                            tokenizer.Move();

                        while (tokenizer.IsBlank() && !tokenizer.IsBreak())
                            tokenizer.Move();
                    }
                    break;
            }

            return true;
        }
    }
}
