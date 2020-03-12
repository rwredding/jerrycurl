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
            string defaultFile = Path.Combine(Environment.CurrentDirectory, "jerry.rsp");
            string command = args.Options["-c", "--command"]?.Value;

            if (responseFiles.Length == 0 && File.Exists(defaultFile))
                responseFiles = new[] { defaultFile };
            else if (responseFiles.Length == 0)
                throw new RunnerException("Please specify at least one response file with -f|--file or create a default 'jerry.rsp' in the current directory.");

            string[] prefixedFiles = responseFiles.Select(f => f.StartsWith('@') ? f : '@' + f).ToArray();
            string[] argumentList = ToolOptions.ExpandResponseFiles(prefixedFiles).SelectMany(ToolOptions.ToArgumentList).SkipWhile(s => s == "rsp").ToArray();

            if (command != null)
                argumentList = new[] { command }.Concat(argumentList).ToArray();

            if (argumentList.Length == 0)
                throw new RunnerException("No input arguments found.");

            await DotNetJerryHost.Main(argumentList);
        }
    }
}