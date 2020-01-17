using System.Collections.Generic;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Directives;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorPageData
    {
        public string SourceName { get; set; }
        public string SourceChecksum { get; set; }

        public IList<RazorFragment> Imports { get; set; } = new List<RazorFragment>();

        public RazorFragment Class { get; set; }
        public RazorFragment Model { get; set; }
        public RazorFragment Result { get; set; }
        public RazorFragment Namespace { get; set; }
        public RazorFragment Template { get; set; }

        public IList<InjectDirective> Projections { get; set; } = new List<InjectDirective>();
        public IList<InjectDirective> Injections { get; set; } = new List<InjectDirective>();
        public IList<RazorFragment> Content { get; set; } = new List<RazorFragment>();
    }
}
