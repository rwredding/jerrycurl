using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11.Internal.Enumerators
{
    internal class NameBuffer
    {
        private readonly StringBuilder buffer;
        private readonly DotNotation2 notation;
        private int bufferStart;
        private int bufferEnd;

        public int Index { get; private set; } = -1;
        public string NamePart { get; }

        public NameBuffer(string namePart, DotNotation2 notation)
        {
            this.buffer = new StringBuilder(namePart);
            this.bufferStart = this.buffer.Length;
            this.notation = notation;
            this.NamePart = namePart;
        }

        public void Reset()
        {
            if (this.Index > -1)
            {
                this.buffer.Length = this.bufferStart - 1;
                this.Index = -1;
            }
        }

        public void Increment()
        {
            if (this.Index == -1)
            {
                this.buffer.Length = this.bufferStart++;
                this.buffer.Append(this.notation.IndexBefore);
            }

            this.Index++;
            this.buffer.Length = this.bufferStart;
            this.buffer.Append(this.Index);
            this.buffer.Append(this.notation.IndexAfter);

            this.bufferEnd = this.buffer.Length;
        }

        public string CombineWith(string namePart)
        {
            this.buffer.Length = this.bufferEnd;
            this.buffer.Append(this.notation.Dot);
            this.buffer.Append(namePart);

            return this.buffer.ToString();
        }

        private struct SC
        {
            public string Before { get; set; }
            public string Index { get; set; }
            public string After { get; set; }
        }
    }
}
