using System.IO;
using System.Reflection;
using Jerrycurl.CommandLine;
using Jerrycurl.Reflection;

namespace Jerrycurl.Tools.DotNet.Cli.Commands
{
    internal class RunnerArgs
    {
        private const string ProxyAssemblyName = "dotnet-jerry-proxy";

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
                IsProxy = (Assembly.GetEntryAssembly().GetName().Name == ProxyAssemblyName),
                Proxy = GetProxyArgs(options),
            };
        }

        private static ProxyArgs GetProxyArgs(ToolOptions options)
        {
            string packageName = GetNuGetPackageName(options);
            NuGetVersion version = GetNuGetPackageVersion();

            if (packageName == null || version == null)
                return null;

            string sourcePath = Path.GetDirectoryName(typeof(DotNetJerryHost).Assembly.Location);
            string projectPath = Path.Combine(sourcePath, $"{ProxyAssemblyName}.csproj");
            string binPath = Path.Combine(sourcePath, "built", packageName.ToLower(), "bin");
            string dllPath = Path.Combine(sourcePath, "built", packageName.ToLower(), "obj");
            string intermediatePath = Path.Combine(binPath, $"{ProxyAssemblyName}.dll");

            binPath = binPath.TrimEnd('\\', '/') + '\\';
            intermediatePath = intermediatePath.TrimEnd('\\', '/') + '\\';

            return new ProxyArgs()
            {
                PackageName = packageName,
                PackageVersion = version.IsPrerelease ? version.Version : version.PublicVersion,
                DllPath = dllPath,
                DllName = ProxyAssemblyName,
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
