using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.CommandLine;
using Jerrycurl.Tools.DotNet.Cli.Commands;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal class BuildRunner
    {
        public RunnerArgs Args { get; }

        public BuildRunner(RunnerArgs args)
        {
            this.Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public async Task ExecuteAsync()
        {
            if (this.Args.Proxy == null)
                throw new RunnerException("Please specify a valid vendor using the -v|--vendor parameter. Possible values are 'sqlserver', 'sqlite', 'postgres', 'oracle' or 'mysql'.");

            if (this.IsBuildRequired())
                await this.BuildAsync();

            await this.RunAsync();
        }

        private bool IsBuildRequired() => !File.Exists(this.Args.Proxy.DllPath);

        private async Task BuildAsync()
        {
            Program.WriteLine($"Fetching {this.Args.Proxy.PackageName} v{this.Args.Proxy.PackageVersion}...", ConsoleColor.Yellow);

            string[] arguments = new[]
            {
                "build",
                this.Args.Proxy.ProjectPath,
                "--configuration", "Release",
                "--verbosity", "quiet",
                $"-p:OutputPath={this.Args.Proxy.BinPath}",
                $"-p:VendorPackage={this.Args.Proxy.PackageName}",
                $"-p:VendorVersion={this.Args.Proxy.PackageVersion}",
                $"-p:AssemblyName={this.Args.Proxy.DllName}",
            };

            await ToolRunner.RunAsync("dotnet", arguments);
        }

        private async Task RunAsync()
        {
            List<string> arguments = new List<string>()
            {
                this.Args.Proxy.DllPath,
                this.Args.Command,
            };

            arguments.AddRange(this.Args.Options.ToArgumentList());

            try
            {
                await ToolRunner.RunAsync("dotnet", arguments, capture: false);
            }
            catch (ToolException) { }
        }

    }
}
