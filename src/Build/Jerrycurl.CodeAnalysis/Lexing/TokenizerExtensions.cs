using System;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public static class TokenizerExtensions
    {
        public static bool IsBlank(this Tokenizer tokenizer)
        {
            return char.IsWhiteSpace(tokenizer[0] ?? '\0');
        }

        public static bool Blank(this Tokenizer tokenizer)
        {
            if (tokenizer.IsBlank())
            {
                tokenizer.Move(1);

                return true;
            }

            return false;
        }

        public static bool Blanks(this Tokenizer tokenizer)
        {
            while (tokenizer.IsBlank())
                tokenizer.Move();

            return tokenizer.Length > 0;
        }

        public static bool IsBreak(this Tokenizer tokenizer)
        {
            if (tokenizer[0] == '\n' || tokenizer[0] == '\r')
                return true;

            return false;
        }

        public static bool Char(this Tokenizer tokenizer, char c)
        {
            if (tokenizer[0] == c)
            {
                tokenizer.Move(1);

                return true;
            }

            return false;
        }

        public static bool Is(this Tokenizer tokenizer, ISymbol symbol)
        {
            int len = tokenizer.Length;

            if (!tokenizer.Eof && symbol.Parse(tokenizer))
            {
                tokenizer.Length = len;

                return true;
            }

            return false;
        }

        public static bool Sym(this Tokenizer tokenizer, ISymbol symbol) => (!tokenizer.Eof && symbol.Parse(tokenizer));

        public static bool Many(this Tokenizer tokenizer, Func<Tokenizer, bool> predicate)
        {
            if (tokenizer.Eof || !predicate(tokenizer))
                return false;

            bool result = true;

            while (!tokenizer.Eof && result)
                result = predicate(tokenizer);

            return true;
        }

        public static bool IsString(this Tokenizer tokenizer, string s, bool caseSensitive = true)
        {
            if (s.Length == 0)
                return true;

            for (int i = 0; i < s.Length; i++)
            {
                if (caseSensitive && tokenizer[i] != null && tokenizer[i] != s[i])
                    return false;
                else if (!caseSensitive && tokenizer[i] != null && char.ToUpper(tokenizer[i].Value) != char.ToUpper(s[i]))
                    return false;
                else if (tokenizer[i] == null)
                    return false;
            }

            return true;
        }

        public static bool String(this Tokenizer tokenizer, string s, bool caseSensitive = true)
        {
            if (tokenizer.IsString(s, caseSensitive))
            {
                tokenizer.Move(s.Length);

                return true;
            }

            return false;
        }

        public static bool Func(this Tokenizer tokenizer, Func<Tokenizer, bool> func)
        {
            int len = tokenizer.Length;

            if (func(tokenizer))
                return true;

            tokenizer.Length = len;

            return false;
        }
    }
}
