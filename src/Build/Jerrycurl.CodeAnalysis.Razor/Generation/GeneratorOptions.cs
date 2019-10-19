using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Directives;

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
