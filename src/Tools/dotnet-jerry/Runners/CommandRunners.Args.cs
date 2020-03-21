using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.CommandLine;
using Jerrycurl.Tools.DotNet.Cli.Commands;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal static partial class CommandRunners
    {
        public static void Args(RunnerArgs args)
        {
            IEnumerable<string> argumentList = args.Options.Skip(1).SelectMany(opt => opt.ToArgumentList());

            Console.WriteLine(string.Join(" ", argumentList.Select(ToolOptions.Escape)));
        }
    }
}
