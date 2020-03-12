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
        public static void Help(RunnerArgs args)
        {
            if (args.Options.Commands.Length == 1 || args.Options.Commands[1] == "help")
            {
                Program.WriteHeader();

                Program.WriteLine("Usage: jerry [command] [options]");
                Program.WriteLine();
                Program.WriteLine("Execute a command with the Jerrycurl CLI.");
                Program.WriteLine();
                Program.WriteLine("Commands:");
                Program.WriteLine("  scaffold                    Generate C# classes from an existing database.");
                Program.WriteLine("  tp                          Transpile .cssql files into C# files.");
                Program.WriteLine("  info                        Show information about a specific database connector.");
                Program.WriteLine("  help [command]              Show help information about one of the above.");
                Program.WriteLine();
            }
            else
            {
                switch (args.Options.Commands[1])
                {
                    case "scaffold":
                        TranspileHelp();
                        break;
                    case "tp":
                        ScaffoldHelp();
                        break;
                    default:
                        throw new RunnerException($"Invalid command '{args.Options.Commands[1]}'.");
                }
            }
        }

        private static void ScaffoldHelp()
        {
            Program.WriteHeader();

            Program.WriteLine("Usage: jerry scaffold --v <moniker> --c <connection> [options]");
            Program.WriteLine();
            Program.WriteLine("Generate a C# object model from an existing database.");
            Program.WriteLine();
            Program.WriteLine("Options:");
            Program.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
            Program.WriteLine("                                be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
            Program.WriteLine("                                or 'mysql'.");
            Program.WriteLine("  -c, --connection <cs>       Connection string used to connect to database.");
            Program.WriteLine("  -ns, --namespace <ns>       Namespace to place scaffolded C# classes in.");
            Program.WriteLine("  -o, --output <file>         Path to scaffold .cs files into. Writes one");
            Program.WriteLine("                                file per class unless specified with .cs");
            Program.WriteLine("                                extension. Defaults to Database.cs.");
            Program.WriteLine();
        }

        private static void TranspileHelp()
        {
            Program.WriteHeader();

            Program.WriteLine("Usage: jerry tp [options]");
            Program.WriteLine();
            Program.WriteLine("Transpile .cssql files into C# files.");
            Program.WriteLine();
            Program.WriteLine("Options:");
            Program.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
            Program.WriteLine("                                be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
            Program.WriteLine("                                or 'mysql'.");
            Program.WriteLine("  -c, --connection <cs>       Connection string used to connect to database.");
            Program.WriteLine("  -ns, --namespace <ns>       Namespace to place scaffolded C# classes in.");
            Program.WriteLine("  -o, --output <file>         Path to scaffold .cs files into. Writes one");
            Program.WriteLine("                                file per class unless specified with .cs");
            Program.WriteLine("                                extension. Defaults to Database.cs.");
            Program.WriteLine();
        }

        private static void InfoHelp()
        {
            Program.WriteHeader();

            Program.WriteLine("Usage: jerry info --v <moniker>");
            Program.WriteLine();
            Program.WriteLine("Show information about a specific database connector.");
            Program.WriteLine();
            Program.WriteLine("Options:");
            Program.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
            Program.WriteLine("                                be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
            Program.WriteLine("                                or 'mysql'.");
            Program.WriteLine();
        }

        public static void Invalid(RunnerArgs args)
        {
            Program.WriteHeader();
            Program.WriteLine("Usage: jerry [command] [options]");
            Program.WriteLine("Use 'jerry help' to show options.");

            if (string.IsNullOrEmpty(args.Command))
                throw new RunnerException("No command specified.");
            else
                throw new RunnerException($"Invalid command '{args.Command}'.");
        }
    }
}
