using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public class Literal : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            if (tokenizer.Sym(Facts.CharLiteral.Start))
            {
                while (!tokenizer.IsBreak() && !tokenizer.Eof)
                {
                    if (tokenizer[0] == '\\' && tokenizer[1] == '\'')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '\'')
                    {
                        tokenizer.Move();

                        return true;
                    }
                    else
                        tokenizer.Move();
                }

                return true;
            }
            else if (tokenizer.Sym(Facts.Literal.Start))
            {
                while (!tokenizer.IsBreak() && !tokenizer.Eof)
                {
                    if (tokenizer[0] == '\\' && tokenizer[1] == '"')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '"')
                    {
                        tokenizer.Move();

                        return true;
                    }
                    else
                        tokenizer.Move();
                }

                return true;
            }
            else if (tokenizer.Sym(Facts.VerbatimLiteral.Start))
            {
                while (!tokenizer.Eof)
                {
                    if (tokenizer[0] == '"' && tokenizer[1] == '"')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '"')
                    {
                        tokenizer.Move();

                        return true;
                    }
                    else
                        tokenizer.Move();
                }

                return true;
            }
            else if (tokenizer.Sym(Facts.InterpolatedLiteral.Start))
            {
                while (!tokenizer.IsBreak() && !tokenizer.Eof)
                {
                    if (tokenizer[0] == '\\' && tokenizer[1] == '"')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '{' && tokenizer[1] == '{')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '{')
                        this.ParseInterpolation(tokenizer);
                    else if (tokenizer[0] == '"')
                    {
                        tokenizer.Move();

                        return true;
                    }
                    else
                        tokenizer.Move();
                }

                return true;
            }
            else if (tokenizer.Sym(Facts.InterpolatedVerbatimLiteral.Start))
            {
                while (!tokenizer.Eof)
                {
                    if (tokenizer[0] == '"' && tokenizer[1] == '"')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '{' && tokenizer[1] == '{')
                        tokenizer.Move(2);
                    else if (tokenizer[0] == '{')
                        this.ParseInterpolation(tokenizer);
                    else if (tokenizer[0] == '"')
                    {
                        tokenizer.Move();

                        return true;
                    }
                    else
                        tokenizer.Move();
                }
            }

            return false;
        }

        private bool ParseInterpolation(Tokenizer tokenizer)
        {
            if (!tokenizer.Sym(Facts.Statement.Start))
                return false;

            int balance = 1;

            while (!tokenizer.Eof && (balance > 0 || tokenizer[0] != '"'))
            {
                if (tokenizer.Sym(Facts.Statement.Start))
                    balance++;
                else if (tokenizer.Sym(Facts.Statement.End))
                    balance--;

                tokenizer.Move();
            }

            return true;
        }
    }
}