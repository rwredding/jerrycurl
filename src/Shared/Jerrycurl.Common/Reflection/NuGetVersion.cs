namespace Jerrycurl.Reflection
{
    internal class NuGetVersion
    {
        public string CommitHash { get; set; }
        public string Version { get; set; }
        public string PublicVersion { get; set; }
        public bool IsPrerelease { get; set; }
    }
}
