using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;

namespace Jerrycurl.CodeAnalysis.Projection
{
    public struct ProjectionSpan
    {
        public SourceSpan From { get; }
        public SourceSpan To { get; }

        public ProjectionSpan(SourceSpan fromSpan, SourceSpan toSpan)
        {
            this.From = fromSpan;
            this.To = toSpan;
        }

        public ProjectionSpan(SourceSpan fromSpan, int projectionStart)
            : this(fromSpan, new SourceSpan(projectionStart, fromSpan.Length))
        {

        }

        public override string ToString() => $"[{this.From.Start}->{this.To.Start}:{this.From.Length}]";
    }
}
