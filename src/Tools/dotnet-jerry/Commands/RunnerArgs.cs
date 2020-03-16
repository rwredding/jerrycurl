using System.IO;
using System.Reflection;
using Jerrycurl.CommandLine;
using Jerrycurl.Facts;
using Jerrycurl.Reflection;
using Jerrycurl.Tools.DotNet.Cli.ComponentModel;

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
                IsProxy = IsProxyRunner(),
                Proxy = GetProxyArgs(options),
            };
        }

        private static ProxyArgs GetProxyArgs(ToolOptions options)
        {
            string moniker = options["-v", "--vendor"]?.Value;
            string packageName = DatabaseFacts.GetToolsNuGetPackage(moniker);
            string packageVersion = GetNuGetPackageVersionString();

            if (packageName == null || packageVersion == null)
                return null;

            string sourcePath = Path.GetDirectoryName(typeof(DotNetJerryHost).Assembly.Location);
            string projectPath = Path.Combine(sourcePath, $"{ProxyAssemblyName}.csproj");
            string binPath = Path.Combine(sourcePath, "built", moniker, "bin");
            string intermediatePath = Path.Combine(sourcePath, "built", moniker, "obj");
            string dllPath = Path.Combine(binPath, $"{ProxyAssemblyName}.dll");

            binPath = binPath.TrimEnd('\\', '/') + '\\';
            intermediatePath = intermediatePath.TrimEnd('\\', '/') + '\\';

            return new ProxyArgs()
            {
                PackageName = packageName,
                PackageVersion = packageVersion,
                DllPath = dllPath,
                DllName = ProxyAssemblyName,
                ProjectPath = projectPath,
                BinPath = binPath,
                IntermediatePath = intermediatePath,
            };
        }

        private static bool IsProxyRunner() => (Assembly.GetEntryAssembly().GetCustomAttribute<ProxyHostAttribute>() != null);

        private static string GetNuGetPackageVersionString()
        {
#pragma warning disable CS0162
            NuGetVersion version = GetNuGetPackageVersion();

            if (version == null)
                return null;
            else if (ThisAssembly.IsPublicRelease)
                return version.PublicVersion;
            else
                return version.Version;

#pragma warning restore CS0162
        }

        public static NuGetVersion GetNuGetPackageVersion() => typeof(DotNetJerryHost).Assembly.GetNuGetPackageVersion();
    }
}
