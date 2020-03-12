using Jerrycurl.Tools.DotNet.Cli;
using System.Threading.Tasks;

class DotNetJerryProxyHost
{
    static Task<int> Main(string[] args) => DotNetJerryHost.Main(args);
}