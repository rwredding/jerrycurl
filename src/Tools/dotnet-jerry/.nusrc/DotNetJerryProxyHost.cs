using Jerrycurl.Tools.DotNet.Cli;
using Jerrycurl.Tools.DotNet.Cli.ComponentModel;
using System.Threading.Tasks;

[assembly: ProxyHost]

class DotNetJerryProxyHost
{
    static Task<int> Main(string[] args) => DotNetJerryHost.Main(args);
}