using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Jerrycurl.CommandLine;
using Jerrycurl.Facts;
using Jerrycurl.IO;
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
            ToolOptions options = ParseArguments(args);

            return new RunnerArgs()
            {
                Command = options.Default.Length > 0 ? options.Default[0] : null,
                Options = options,
                Connection = options["-c", "--connection"]?.Value,
                Namespace = options["-ns", "--namespace"]?.Value,
                Output = options["-o", "--output"]?.Value,
                Verbose = (options["--verbose"] != null),
                IsProxy = IsProxyRunner(),
                Proxy = GetProxyArgs(options),
            };
        }

        private static string[] ApplyDefaultArguments(string[] args)
        {
            string[] checkFor = new[] { "scaffold", "transpile" };

            if (args.Length == 0)
            {
                string[] exists = checkFor.Where(n => File.Exists($"{n}.cli")).ToArray();

                if (exists.Length == 0)
                    return args;
                else if (exists.Length > 1)
                    throw new InvalidOperationException("Multiple default '<command>.cli' files found. Please specify the right one.");
                else
                    return new[] { $"@{exists[0]}.cli" };
            }
            else if (args.Length == 1 && checkFor.Contains(args[0]) && File.Exists($"{args[0]}.cli"))
                return new[] { args[0], $"@{args[0]}.cli" };
            else
                return args;
        }
        private static ToolOptions ParseArguments(string[] args)
        {
            ResponseSettings settings = new ResponseSettings()
            {
                WorkingDirectory = Environment.CurrentDirectory,
                IgnoreComments = true,
                IgnoreMissingFiles = false,
                IgnoreWhitespace = true,
                DefaultExtension = ".cli",
            };

            return ToolOptions.Parse(ApplyDefaultArguments(args), settings);
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
