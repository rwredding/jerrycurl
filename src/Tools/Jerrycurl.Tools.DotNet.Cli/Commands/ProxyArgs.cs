namespace Jerrycurl.Tools.DotNet.Cli.Commands
{
    public class ProxyArgs
    {
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }

        public string BinPath { get; set; }
        public string ProjectPath { get; set; }
        public string DllPath { get; set; }
        public string DllName { get; set; }
    }
}
