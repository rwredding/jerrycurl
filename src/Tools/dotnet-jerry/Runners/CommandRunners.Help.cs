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

                DotNetJerryHost.WriteLine("Usage: jerry [command] [options] [@file]");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Execute a command with the specified options. Specify directly and from file input");
                DotNetJerryHost.WriteLine("using @file[.cli] syntax.");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Commands:");
                DotNetJerryHost.WriteLine("  scaffold                    Generate a C# object model from an existing database.");
                DotNetJerryHost.WriteLine("  transpile                   Transpile a collection of .cssql files into C# classes.");
                DotNetJerryHost.WriteLine("  info                        Show information about a database connector.");
                DotNetJerryHost.WriteLine("  args                        Show all arguments including expanded file inputs.");
                DotNetJerryHost.WriteLine("  help [command]              Show help information about the commands above.");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Examples:");
                DotNetJerryHost.WriteLine("  Generate a C# model with arguments specified in a local 'db.cli' file.");
                DotNetJerryHost.WriteLine("  > jerry scaffold @db");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("  Transpile .cssql files from local directories 'Queries' and 'Commands'.");
                DotNetJerryHost.WriteLine("  > jerry transpile -d Queries -d Commands");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("  Show help for the 'scaffold' command.");
                DotNetJerryHost.WriteLine("  > jerry help scaffold");
                DotNetJerryHost.WriteLine();
            }
            else
            {
                switch (args.Options.Default[1])
                {
                    case "scaffold":
                    case "sf":
                        HelpForScaffold();
                        break;
                    case "transpile":
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
            DotNetJerryHost.WriteLine("  -v, --vendor <moniker>      Vendor used to connect. Can be 'sqlserver', 'sqlite', 'oracle',");
            DotNetJerryHost.WriteLine("                                  'postgres' or 'mysql'.");
            DotNetJerryHost.WriteLine("  -c, --connection <cs>       Connection string used to connect.");
            DotNetJerryHost.WriteLine("  -ns, --namespace <ns>       Namespace to place generated classes in.");
            DotNetJerryHost.WriteLine("  -o, --output <file>         File or directory to generate .cs files into.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  Generate model into 'Database.cs' with specified vendor, connection and namespace.");
            DotNetJerryHost.WriteLine("  > jerry scaffold -v sqlserver -c \"DATABASE=moviedb\" -ns MovieDb.Database");
            DotNetJerryHost.WriteLine("  Generate model into the 'Database' directory using one file per table.");
            DotNetJerryHost.WriteLine("  > jerry scaffold [...] -o .\\Database");
            DotNetJerryHost.WriteLine();
        }

        private static void HelpForTranspile()
        {
            DotNetJerryHost.WriteHeader();

            DotNetJerryHost.WriteLine("Usage: jerry transpile [options]");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Transpile a collection of .cssql files into C# classes.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Options:");
            DotNetJerryHost.WriteLine("  -p, --project          Project directory to resolve relative paths from. Defaults to the");
            DotNetJerryHost.WriteLine("                             current directory.");
            DotNetJerryHost.WriteLine("  -f, --file             Add a file to the project.");
            DotNetJerryHost.WriteLine("  -d, --directory        Add all .cssql files from a specified source directory.");
            DotNetJerryHost.WriteLine("  -ns, --namespace       Root namespace from which to generate sub-namespaces for each .cssql");
            DotNetJerryHost.WriteLine("                             file based on its relative project path.");
            DotNetJerryHost.WriteLine("  -i, --import           Add a namespace import.");
            DotNetJerryHost.WriteLine("  -o, --output           Output directory for .cs files. Defaults to 'obj\\Jerrycurl'");
            DotNetJerryHost.WriteLine("  --no-clean             Do not clean the output directory before transpiling.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  Transpile all .cssql files from the current directory with the specified root namespace.");
            DotNetJerryHost.WriteLine("  > jerry transpile -d . -ns MovieDb.Data");
            DotNetJerryHost.WriteLine("  Transpile .cssql files and import 'MovieDb.Database' namespace.");
            DotNetJerryHost.WriteLine("  > jerry transpile -f Query1.cssql Query2.cssql -i MovieDb.Database");
            DotNetJerryHost.WriteLine();
        }

        private static void HelpForInfo()
        {
            DotNetJerryHost.WriteHeader();

            DotNetJerryHost.WriteLine("Usage: jerry info [options]");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Show information about a specific database connector.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Options:");
            DotNetJerryHost.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
            DotNetJerryHost.WriteLine("                                be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
            DotNetJerryHost.WriteLine("                                or 'mysql'.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  Show information about the Microsoft SQL Server connector.");
            DotNetJerryHost.WriteLine("  > jerry info -v sqlserver");
            DotNetJerryHost.WriteLine();
        }

        public static void HelpForInvalid(RunnerArgs args)
        {
            DotNetJerryHost.WriteHeader();
            DotNetJerryHost.WriteLine("Usage: jerry [command] [options].");
            DotNetJerryHost.WriteLine("Use 'jerry help' to show commands and options.");
            DotNetJerryHost.WriteLine();

            if (string.IsNullOrEmpty(args.Command))
                throw new RunnerException("No command specified.");
            else
                throw new RunnerException($"Invalid command '{args.Command}'.");
        }
    }
}
