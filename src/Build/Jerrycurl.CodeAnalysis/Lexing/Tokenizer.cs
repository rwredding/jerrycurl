using System.Linq;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public class Tokenizer
    {
        public ISource Source { get; }
        public int Length { get; set; }

        public Tokenizer(ISource source)
        {
            this.Source = source;
        }

        public bool Eof => (this[0] == null);
        public bool Bof => (this.Source.Bof && this.Length == 0);

        public char? this[int offset] => this.Source[this.Length + offset];

        public void Move(int length = 1)
        {
            this.Length += length;
        }

        public SourceSpan Accept()
        {
            SourceSpan span = this.Source.Read(this.Length);

            this.Length = 0;

            return span;
        }

        public override string ToString()
        {
            return new string(Enumerable.Range(0, this.Length).Select(i => this.Source[i].Value).ToArray());
        }
    }
}
