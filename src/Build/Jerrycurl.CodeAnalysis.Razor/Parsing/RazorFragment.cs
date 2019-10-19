using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Lexing.Razor;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorFragment
    {
        public SourceSpan? Span { get; set; }
        public string Text { get; set; }
        public string SourceName { get; set; }

        public override string ToString() => $"{this.GetType().Name} {this.Span}: '{this.Text}'";
    }
}
