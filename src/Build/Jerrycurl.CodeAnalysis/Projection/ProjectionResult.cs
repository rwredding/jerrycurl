using Jerrycurl.CodeAnalysis.Lexing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Projection
{
    public class ProjectionResult
    {
        public string Content { get; internal set; }
        public IReadOnlyList<ProjectionSpan> Spans { get; internal set; }
    }
}
