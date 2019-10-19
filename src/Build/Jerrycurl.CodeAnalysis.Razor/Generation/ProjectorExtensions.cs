using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Parsing;

namespace Jerrycurl.CodeAnalysis.Razor.Generation
{
    public static class ProjectorExtensions
    {
        public static Projector WriteError(this Projector projector, RazorFragment fragment, string errorMessage) => projector.WritePragma($"#error {errorMessage}", fragment.Span, fragment.SourceName);
        public static Projector WriteWarning(this Projector projector, RazorFragment fragment, string warningMessage) => projector.WritePragma($"#warning {warningMessage}", fragment.Span, fragment.SourceName);

        public static Projector WritePragma(this Projector projector, RazorFragment fragment, bool terminate = false)
        {
            string text = fragment.Text;

            if (terminate && !text.TrimEnd().EndsWith(";"))
                text += ";";

            return projector.WritePragma(text, fragment.Span, fragment.SourceName);
        }

        public static Projector WritePragma(this Projector projector, string text, SourceSpan? span, string sourceName)
        {
            if (span == null)
                return projector.Write(text);
            else if (sourceName == null)
                return projector.Write(text, span.Value);

            return projector
                    .WriteLine()
                    .Write($"#line {span.Value.Line} {CSharp.Literal(sourceName)}")
                    .WriteLine()
                    .Write(new string(' ', span.Value.Column - 1))
                    .Write(text, span.Value)
                    .WriteLine()
                    .WriteLine("#line default")
                    .WriteLine("#line hidden");

        }
    }
}
