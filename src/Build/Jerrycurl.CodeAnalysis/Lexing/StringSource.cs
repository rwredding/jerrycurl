using System;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public class StringSource : ISource
    {
        private readonly string buffer;

        private int offset = 0;
        private int line = 1;
        private int column = 1;

        public StringSource(string buffer)
        {
            this.buffer = buffer ?? "";
        }

        public char? this[int position]
        {
            get
            {
                if (this.offset + position >= this.buffer.Length)
                    return null;

                return this.buffer[this.offset + position];
            }
        }

        public bool Bof => (this.offset == 0);
        public bool Eof => (this.offset >= this.buffer.Length);

        public override string ToString()
        {
            return this.buffer.Substring(this.offset, Math.Min(20, this.buffer.Length - this.offset)) + " [" + this.offset + "]";
        }

        private void MoveForward(int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (this[i] == '\r' && this[i + 1] == '\n')
                {
                    this.column = 1;
                    this.line++;

                    i++;
                }
                else if (this[i] == '\r' || this[i] == '\n')
                {
                    this.column = 1;
                    this.line++;
                }
                else
                    this.column++;
            }

            this.offset += length;
        }

        public string GetText(SourceSpan span) => this.buffer.Substring(span.Start, span.Length);
        public string GetText() => this.buffer;


        public SourceSpan Read(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            SourceSpan newSpan = new SourceSpan(this.offset, length, this.line, this.column);

            this.MoveForward(length);

            return newSpan;
        }

        public void Discard(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            this.MoveForward(length);
        }
    }
}
