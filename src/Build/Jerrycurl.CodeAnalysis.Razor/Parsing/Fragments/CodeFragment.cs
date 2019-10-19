using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Razor.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp;
using Jerrycurl.CodeAnalysis.Razor.Lexing.Razor;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing.Fragments
{
    public class CodeFragment : RazorFragment
    {
        public CSharpType CodeType { get; set; }
    }
}
