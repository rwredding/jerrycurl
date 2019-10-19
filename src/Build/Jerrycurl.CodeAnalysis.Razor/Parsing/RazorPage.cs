using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Razor.Parsing;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorPage
    {
        public string Path { get; internal set; }
        public string ProjectPath { get; internal set; }

        public RazorPageData Data { get; internal set; }
    }
}
