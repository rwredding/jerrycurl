using System;

namespace Jerrycurl.CodeAnalysis.Lexing
{
    public struct SourceSpan : IEquatable<SourceSpan>
    {
        public int Start { get; }
        public int End => this.Start + this.Length;
        public int Column { get; }
        public int Line { get; }
        public int Length { get; }
        public bool IsEmpty => this.Length == 0;

        public static SourceSpan Empty(int start) => new SourceSpan(start, 0);

        public SourceSpan(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            this.Start = start;
            this.Length = length;
            this.Column = 0;
            this.Line = 0;
        }

        public SourceSpan(int start, int length, int line, int column)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (start + length < start)
                throw new ArgumentOutOfRangeException(nameof(length));

            this.Start = start;
            this.Length = length;
            this.Line = line;
            this.Column = column;
        }

        public bool Contains(int position) => (this.Start <= position && this.End >= position);

        public override string ToString() => $"[{this.Start}..{this.End}]";

        public bool Equals(SourceSpan other) => (this.Start == other.Start && this.Length == other.Length);
        public override bool Equals(object obj) => (obj is SourceSpan other && this.Equals(other));
        public override int GetHashCode() => ((this.Start << 8) & this.Length);
    }
}
