using Jerrycurl.CodeAnalysis.Razor.Lexing.Razor;
using Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing
{
    public class CSharpBlock : IRule
    {
        public RazorType RazorType { get; }
        public CSharpType Type { get; private set; }

        public CSharpBlock(RazorType razorType)
        {
            this.RazorType = razorType;
        }

        public bool Parse(Lexer lexer)
        {
            switch (this.RazorType)
            {
                case RazorType.Comment:
                    this.Type = CSharpType.Comment;

                    lexer.Yield(new RazorComment());
                    break;
                case RazorType.Expression:
                    this.Type = CSharpType.Expression;

                    lexer.Yield(new BlockContent(Facts.Expression));
                    break;
                case RazorType.Statement:
                    this.Type = CSharpType.Statement;

                    lexer.Yield(new BlockContent(Facts.Statement));
                    break;
                case RazorType.Reserved:
                    this.Type = CSharpType.Statement;

                    lexer.Yield(new BlockStart());
                    lexer.Yield(new BlockEnd());
                    break;
                case RazorType.Inline when lexer.Yield(new KeywordBlock()):
                    this.Type = CSharpType.Statement;
                    break;
                case RazorType.Inline:
                    this.Type = CSharpType.Expression;

                    lexer.Yield(new Inline());
                    break;
                case RazorType.Model:
                case RazorType.Result:
                case RazorType.Import:
                case RazorType.Template:
                case RazorType.Namespace:
                case RazorType.Class:
                    this.Type = CSharpType.Directive;

                    lexer.Yield(new Argument(ArgumentType.Line, true));
                    break;
                case RazorType.Projection:
                case RazorType.Injection:
                    this.Type = CSharpType.Directive;

                    lexer.Yield(new Argument(ArgumentType.Word, true));
                    lexer.Yield(new Argument(ArgumentType.Line, false));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown type '{this.RazorType}'.");
            }

            return true;
        }
    }
}
