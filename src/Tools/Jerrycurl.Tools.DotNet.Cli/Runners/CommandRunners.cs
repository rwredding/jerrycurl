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
        public static void Info(RunnerArgs info, InfoCommand command)
        {
            if (command == null)
                throw new RunnerException("Invalid command object.");

            Program.WriteHeader();
            Program.WriteLine($"Package: {info.Proxy.PackageName}");
            Program.WriteLine($"Connector: {command.Connector} v{command.ConnectorVersion}");
        }

        public static void Meow()
        {
            Program.WriteLine();
            Program.WriteLine(@"   |\---/|");
            Program.WriteLine(@"   | o_o |");
            Program.WriteLine(@"    \_^_/ ");
            Program.WriteLine();
        }
    }
}
