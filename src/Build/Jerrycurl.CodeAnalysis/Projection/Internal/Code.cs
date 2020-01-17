using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Projection.Internal
{
    internal class Code : ISymbol
    {
        public bool Parse(Tokenizer tokenizer)
        {
            if (tokenizer[0] != '$')
                return false;

            tokenizer.Move();

            while (!tokenizer.Eof && tokenizer[0] != '$')
                tokenizer.Move();

            if (!tokenizer.Eof)
                tokenizer.Move();

            return (tokenizer.Length > 0);
        }
    }
}
