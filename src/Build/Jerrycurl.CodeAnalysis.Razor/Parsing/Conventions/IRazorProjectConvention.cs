using System.Collections.Generic;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing.Conventions
{
    public interface IRazorProjectConvention
    {
        void Apply(RazorProject project, IList<RazorPage> result);
    }
}
