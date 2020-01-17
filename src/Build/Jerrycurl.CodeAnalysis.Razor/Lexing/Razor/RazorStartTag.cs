using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.Razor
{
    public class RazorStartTag : ISymbol
    {
        public RazorType Type { get; private set; }

        public bool Parse(Tokenizer tokenizer)
        {
            if (tokenizer[0] == '{' && (tokenizer.Eof || tokenizer[1] != '{'))
            {
                this.Type = RazorType.Reserved;

                return true;
            }
            else if (tokenizer[0] == '}' && (tokenizer.Eof || tokenizer[1] != '}'))
            {
                this.Type = RazorType.Reserved;

                return true;
            }
            else if (tokenizer[0] != '@' || tokenizer[1] == '@')
                return false;

            tokenizer.Move();

            if (tokenizer.Char('{'))
                this.Type = RazorType.Statement;
            else if (tokenizer.Char('('))
                this.Type = RazorType.Expression;
            else if (tokenizer.Char('*'))
                this.Type = RazorType.Comment;
            else if (tokenizer.Func(t => this.ParseDirective(t, "model")))
                this.Type = RazorType.Model;
            else if (tokenizer.Func(t => this.ParseDirective(t, "result")))
                this.Type = RazorType.Result;
            else if (tokenizer.Func(t => this.ParseDirective(t, "namespace")))
                this.Type = RazorType.Namespace;
            else if (tokenizer.Func(t => this.ParseDirective(t, "class")))
                this.Type = RazorType.Class;
            else if (tokenizer.Func(t => this.ParseDirective(t, "import")))
                this.Type = RazorType.Import;
            else if (tokenizer.Func(t => this.ParseDirective(t, "inject")))
                this.Type = RazorType.Injection;
            else if (tokenizer.Func(t => this.ParseDirective(t, "project")))
                this.Type = RazorType.Projection;
            else if (tokenizer.Func(t => this.ParseDirective(t, "template")))
                this.Type = RazorType.Template;
            else
                this.Type = RazorType.Inline;

            return true;
        }

        private bool ParseDirective(Tokenizer tokenizer, string directive) => tokenizer.String(directive) && (tokenizer.Eof || !tokenizer.IsIdentifier());
    }
}
