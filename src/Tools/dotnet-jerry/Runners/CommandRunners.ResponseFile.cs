using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Generation;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.CommandLine;
using Jerrycurl.IO;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Scaffolding;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal static partial class CommandRunners
    {
        public static async Task ResponseFileAsync(RunnerArgs args)
        {
            string[] responseFiles = args.Options["-f", "--file"]?.Values ?? Array.Empty<string>();
            string command = args.Options["-c", "--command"]?.Value;

            if (responseFiles.Length == 0)
            {
                responseFiles = GetDefaultFiles();

                if (responseFiles.Length == 0)
                    throw new RunnerException("Please specify at least one response file with -f|--file or create a default '<command>.cli' in the current directory.");
                else if (responseFiles.Length > 1)
                    throw new RunnerException("Multiple default '<command>.cli' files found. Please specify the right one with the -c|--command argument.");
            }

            string[] prefixedFiles = responseFiles.Select(f => f.StartsWith('@') ? f : '@' + f).ToArray();
            string[] argumentList = ResponseFile.ExpandStrings(prefixedFiles).SelectMany(ToolOptions.ToArgumentList).SkipWhile(s => s == "cli").ToArray();

            if (command != null && command != "cli")
                argumentList = new[] { command }.Concat(argumentList).ToArray();

            if (argumentList.Length == 0)
                throw new RunnerException("No input arguments found.");

            await DotNetJerryHost.Main(argumentList);

            string[] GetDefaultFiles()
            {
                if (command == null)
                {
                    string[] defaultNames = new[] { "scaffold.cli", "info.cli", "transpile.cli" };

                    return defaultNames.Where(File.Exists).ToArray();

                }
                else if (command != "cli" && File.Exists($"{command}.cli"))
                    return new[] { $"{command}.cli" };

                return Array.Empty<string>();
            }
        }
    }
}