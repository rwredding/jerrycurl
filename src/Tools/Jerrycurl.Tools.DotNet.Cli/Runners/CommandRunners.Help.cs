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
            if (args.Options.Default.Length == 1 || args.Options.Default[1] == "help")
            {
                Program.WriteHeader();

                Program.WriteLine("Usage: jerry [command] [options]");
                Program.WriteLine();
                Program.WriteLine("Execute a command with the Jerrycurl CLI.");
                Program.WriteLine();
                Program.WriteLine("Commands:");
                Program.WriteLine("  scaffold                    Generate a C# object model from an existing database.");
                Program.WriteLine("  tp                          Transpile .cssql files into C# classes.");
                Program.WriteLine("  info                        Show information about a database connector.");
                Program.WriteLine("  help [command]              Show help information about one of the above.");
                Program.WriteLine();
            }
            else
            {
                switch (args.Options.Default[1])
                {
                    case "scaffold":
                        HelpForScaffold(); 
                        break;
                    case "tp":
                        HelpForTranspile();
                        break;
                    case "info":
                        HelpForInfo();
                        break;
                    default:
                        throw new RunnerException($"Invalid command '{args.Options.Default[1]}'.");
                }
            }
        }

        private static void HelpForScaffold()
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
            Program.WriteLine("                                extension. Defaults to Database.cs."); Program.WriteLine();
            Program.WriteLine("Examples:");
            Program.WriteLine("  jerry scaffold -v sqlserver -c \"SERVER=.;DATABASE=moviedb\" -ns MovieDb.Data.Database");
            Program.WriteLine();
        }

        private static void HelpForTranspile()
        {
            Program.WriteHeader();

            Program.WriteLine("Usage: jerry tp [options]");
            Program.WriteLine();
            Program.WriteLine("Transpile .cssql files into C# classes.");
            Program.WriteLine();
            Program.WriteLine("Options:");
            Program.WriteLine("  -pd, --project-dir     Lorem ipsum dolor sit amet.");
            Program.WriteLine("  -f, --file             Lorem ipsum dolor sit amet.");
            Program.WriteLine("  -d, --directory        Lorem ipsum dolor sit amet.");
            Program.WriteLine("  -ns, --namespace       Lorem ipsum dolor sit amet.");
            Program.WriteLine("  -i, --import           Lorem ipsum dolor sit amet.");
            Program.WriteLine("  -o, --output           Lorem ipsum dolor sit amet.");
            Program.WriteLine("  --no-clean             Lorem ipsum dolor sit amet.");
            Program.WriteLine();
            Program.WriteLine("Examples:");
            Program.WriteLine("  jerry tp -d . -ns MovieDb.Data");
            Program.WriteLine();
        }

        private static void HelpForInfo()
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
            Program.WriteLine("Examples:");
            Program.WriteLine("  jerry info -v sqlserver");
            Program.WriteLine();
        }

        public static void Invalid(RunnerArgs args)
        {
            Program.WriteHeader();
            Program.WriteLine("Usage: jerry [command] [options]");
            Program.WriteLine("Use 'jerry help [command]' to show options.");

            if (string.IsNullOrEmpty(args.Command))
                throw new RunnerException("No command specified.");
            else
                throw new RunnerException($"Invalid command '{args.Command}'.");
        }
    }
}
