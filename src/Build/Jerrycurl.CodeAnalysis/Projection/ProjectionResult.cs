using System.Collections.Generic;

namespace Jerrycurl.CodeAnalysis.Projection
{
    public class ProjectionResult
    {
        public string Content { get; internal set; }
        public IReadOnlyList<ProjectionSpan> Spans { get; internal set; }
    }
}
