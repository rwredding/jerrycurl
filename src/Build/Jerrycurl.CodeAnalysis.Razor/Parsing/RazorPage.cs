namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorPage
    {
        public string Path { get; internal set; }
        public string ProjectPath { get; internal set; }
        public string IntermediatePath { get; internal set; }

        public RazorPageData Data { get; internal set; }

        public override string ToString() => this.ProjectPath ?? this.Path;
    }
}
