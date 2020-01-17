using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public sealed class Lexer : IEnumerable<Lexeme>
    {
        private readonly List<Token> tokens = new List<Token>();
        private readonly List<Lexeme> lexemes = new List<Lexeme>();

        public ISource Source { get; }

        public Lexer(ISource source)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public IEnumerable<Token> Tokenize() => this.tokens;
        public bool Eof => this.Source.Eof;

        public bool Yield(IRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            Lexer lexer = new Lexer(this.Source);

            if (rule is SymbolRule symbol && rule.Parse(lexer))
            {
                Token[] tokens = new[] { symbol.Token };
                Lexeme[] lexemes = lexer.ToArray();

                this.tokens.AddRange(tokens);
                this.lexemes.Add(this.CreateLexeme(rule, lexemes, new[] { symbol.Token }));
                this.lexemes.AddRange(lexemes);

                return true;
            }
            else if (rule.Parse(lexer))
            {
                Token[] tokens = lexer.Tokenize().ToArray();
                Lexeme[] lexemes = lexer.ToArray();

                this.tokens.AddRange(tokens);
                this.lexemes.Add(this.CreateLexeme(rule, lexemes, tokens));
                this.lexemes.AddRange(lexemes);

                return true;
            }

            return false;
        }

        private Lexeme CreateLexeme(IRule rule, IEnumerable<Lexeme> lexemes, IEnumerable<Token> tokens)
        {
            Token first = tokens.FirstOrDefault();

            if (first == null)
            {
                return new Lexeme()
                {
                    Rule = rule,
                    Tokens = tokens,
                    Lexemes = lexemes,
                    Span = this.Source.Read(0),
                };
            }
            else
            {
                return new Lexeme()
                {
                    Rule = rule,
                    Tokens = tokens,
                    Lexemes = lexemes,
                    Span = new SourceSpan(first.Span.Start, tokens.Sum(t => t.Span.Length), first.Span.Line, first.Span.Column),
                };
            }
        }

        public IEnumerator<Lexeme> GetEnumerator() => this.lexemes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
