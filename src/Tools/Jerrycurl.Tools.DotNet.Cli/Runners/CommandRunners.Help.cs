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
            if (args.Options.Default.Length == 0)
                HelpForInvalid(args);
            else if (args.Options.Default.Length == 1 || args.Options.Default[1] == "help")
            {
                DotNetJerryHost.WriteHeader();

                DotNetJerryHost.WriteLine("Usage: jerry [command] [options]");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Execute a command with the Jerrycurl CLI.");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Commands:");
                DotNetJerryHost.WriteLine("  scaffold                    Generate a C# object model from an existing database.");
                DotNetJerryHost.WriteLine("  tp                          Transpile .cssql files into C# classes.");
                DotNetJerryHost.WriteLine("  info                        Show information about a database connector.");
                DotNetJerryHost.WriteLine("  help [command]              Show help information about one of the above.");
                DotNetJerryHost.WriteLine();
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
            DotNetJerryHost.WriteHeader();

            DotNetJerryHost.WriteLine("Usage: jerry scaffold [options]");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Generate a C# object model from an existing database.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Options:");
            DotNetJerryHost.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
            DotNetJerryHost.WriteLine("                                be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
            DotNetJerryHost.WriteLine("                                or 'mysql'.");
            DotNetJerryHost.WriteLine("  -c, --connection <cs>       Connection string used to connect to database.");
            DotNetJerryHost.WriteLine("  -ns, --namespace <ns>       Namespace to place scaffolded C# classes in.");
            DotNetJerryHost.WriteLine("  -o, --output <file>         Path to scaffold .cs files into. Writes one");
            DotNetJerryHost.WriteLine("                                file per class unless specified with .cs");
            DotNetJerryHost.WriteLine("                                extension. Defaults to Database.cs."); DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  jerry scaffold -v sqlserver -c \"SERVER=.;DATABASE=moviedb\" -ns MovieDb.Data.Database");
            DotNetJerryHost.WriteLine();
        }

        private static void HelpForTranspile()
        {
            DotNetJerryHost.WriteHeader();

            DotNetJerryHost.WriteLine("Usage: jerry tp [options]");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Transpile .cssql files into C# classes.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Options:");
            DotNetJerryHost.WriteLine("  -p, --project          Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine("  -f, --file             Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine("  -d, --directory        Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine("  -ns, --namespace       Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine("  -i, --import           Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine("  -o, --output           Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine("  --no-clean             Lorem ipsum dolor sit amet.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  jerry tp -d . -ns MovieDb.Data");
            DotNetJerryHost.WriteLine();
        }

        private static void HelpForInfo()
        {
            DotNetJerryHost.WriteHeader();

            DotNetJerryHost.WriteLine("Usage: jerry info --v <moniker>");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Show information about a specific database connector.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Options:");
            DotNetJerryHost.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
            DotNetJerryHost.WriteLine("                                be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
            DotNetJerryHost.WriteLine("                                or 'mysql'.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  jerry info -v sqlserver");
            DotNetJerryHost.WriteLine();
        }

        public static void HelpForInvalid(RunnerArgs args)
        {
            DotNetJerryHost.WriteHeader();
            DotNetJerryHost.WriteLine("Usage: jerry [command] [options]");
            DotNetJerryHost.WriteLine("Use 'jerry help' to show commands and options.");

            if (string.IsNullOrEmpty(args.Command))
                throw new RunnerException("No command specified.");
            else
                throw new RunnerException($"Invalid command '{args.Command}'.");
        }
    }
}
