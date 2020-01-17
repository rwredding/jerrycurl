using System.Collections.Generic;
using Jerrycurl.CodeAnalysis.Razor.Parsing;

namespace Jerrycurl.CodeAnalysis.Razor.Generation
{
    public class GeneratorOptions
    {
        public IList<RazorFragment> Imports { get; set; } = new List<RazorFragment>();
        public RazorFragment Class { get; set; }
        public RazorFragment Namespace { get; set; }

        public string TemplateCode { get; set; }
        public string SourceName { get; set; }
    }
}
