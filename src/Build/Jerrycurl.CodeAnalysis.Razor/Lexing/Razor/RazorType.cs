namespace Jerrycurl.CodeAnalysis.Razor.Lexing.Razor
{
    public enum RazorType
    {
        None = 0,
        Statement = 1,
        Expression = 2,
        Inline = 3,
        Comment = 4,
        Reserved = 13,

        // directives
        Result = 5,
        Model = 6,
        Injection = 7,
        Projection = 8,
        Template = 9,
        Class = 10,
        Namespace = 11,
        Import = 12,
    }
}
