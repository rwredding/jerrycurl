using Jerrycurl.CodeAnalysis.Razor.ProjectSystem.Conventions;

namespace Jerrycurl.CodeAnalysis.Razor.ProjectSystem
{
    public static class RazorProjectConventions
    {
        public const string DefaultIntermediateDirectory = "obj\\Jerrycurl";

        public static IRazorProjectConvention[] Default { get; set; } = new IRazorProjectConvention[]
        {
            new RazorNamingConvention(),
            new RazorImportConvention(),
        };
    }
}
