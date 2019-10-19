using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing.Conventions
{
    public interface IRazorProjectConvention
    {
        void Apply(RazorProject project, IList<RazorPage> result);
    }
}
