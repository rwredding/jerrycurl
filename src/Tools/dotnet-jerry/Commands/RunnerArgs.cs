using System.IO;
using System.Reflection;
using Jerrycurl.CommandLine;
using Jerrycurl.Reflection;

namespace Jerrycurl.Tools.DotNet.Cli.Commands
{
    internal class RunnerArgs
    {
        public string Command { get; private set; }
        public string Connection { get; private set; }
        public string Namespace { get; private set; }
        public string Output { get; private set; }
        public bool Verbose { get; private set; }
        public bool IsProxy { get; private set; }

        public ProxyArgs Proxy { get; private set; }
        public ToolOptions Options { get; set; }

        public static RunnerArgs FromCommandLine(string[] args)
        {
            ToolOptions options = ToolOptions.Parse(args);

            return new RunnerArgs()
            {
                Command = args.Length > 0 ? args[0] : null,
                Options = options,
                Connection = options["-c", "--connection"]?.Value,
                Namespace = options["-ns", "--namespace"]?.Value,
                Output = options["-o", "--output"]?.Value,
                Verbose = (options["--verbose"] != null),
                IsProxy = (Assembly.GetEntryAssembly().GetName().Name == "dotnet-jerry-proxy"),
                Proxy = GetProxyArgs(options),
            };
        }

        private static ProxyArgs GetProxyArgs(ToolOptions options)
        {
            string packageName = GetNuGetPackageName(options);

            if (packageName == null)
                return null;

            NuGetVersion version = GetNuGetPackageVersion();
            string sourcePath = Path.GetDirectoryName(typeof(DotNetJerryHost).Assembly.Location);
            string projectPath = null;
            string binPath = null;
            string dllPath = null;
            string intermediatePath = null;

            if (packageName != null)
            {
                projectPath = Path.Combine(sourcePath, "dotnet-jerry-proxy.csproj");
                binPath = Path.Combine(sourcePath, "built", packageName.ToLower(), "bin");
                intermediatePath = Path.Combine(sourcePath, "built", packageName.ToLower(), "obj");
                dllPath = Path.Combine(binPath, $"dotnet-jerry-proxy.dll");
            }

            return new ProxyArgs()
            {
                PackageName = packageName,
                PackageVersion = version.Version,
                DllPath = dllPath,
                DllName = "dotnet-jerry-proxy",
                ProjectPath = projectPath,
                BinPath = binPath,
                IntermediatePath = intermediatePath,
            };
        }

        private static string GetNuGetPackageName(ToolOptions options)
        {
            string moniker = options["-v", "--vendor"]?.Value;

            switch (moniker)
            {
                case "sqlserver":
                    return "Jerrycurl.Tools.Vendors.SqlServer";
                case "oracle":
                    return "Jerrycurl.Tools.Vendors.Oracle";
                case "mysql":
                    return "Jerrycurl.Tools.Vendors.MySql";
                case "postgres":
                    return "Jerrycurl.Tools.Vendors.Postgres";
                case "sqlite":
                    return "Jerrycurl.Tools.Vendors.Sqlite";
                default:
                    return null;
            }
        }

        public static NuGetVersion GetNuGetPackageVersion() => typeof(DotNetJerryHost).Assembly.GetNuGetPackageVersion();
    }
}
