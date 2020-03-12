using System.Collections.Generic;
using Jerrycurl.CodeAnalysis.Razor.Parsing;

namespace Jerrycurl.CodeAnalysis.Razor.ProjectSystem.Conventions
{
    public interface IRazorProjectConvention
    {
        void Apply(RazorProject project, IList<RazorPage> result);
    }
}
