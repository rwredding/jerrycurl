using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp
{
    public enum CSharpType
    {
        None = 0,
        Statement = 1,
        Expression = 2,
        Directive = 3,
        Comment = 4,
    }
}