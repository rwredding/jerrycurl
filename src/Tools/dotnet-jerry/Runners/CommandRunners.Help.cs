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

                DotNetJerryHost.WriteLine("Usage: jerry [command] [options] [@filename]");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Execute a command with the specified options. Use @[filename]-prefixed arguments to read from");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Commands:");
                DotNetJerryHost.WriteLine("  scaffold                    Generate a C# object model from an existing database.");
                DotNetJerryHost.WriteLine("  transpile                   Transpile a collection of .cssql files into C# classes.");
                DotNetJerryHost.WriteLine("  info                        Show information about a database connector.");
                DotNetJerryHost.WriteLine("  args                        Show all arguments including expanded @files.");
                DotNetJerryHost.WriteLine("  help [command]              Show help information about the commands above.");
                DotNetJerryHost.WriteLine();
                DotNetJerryHost.WriteLine("Arguments:");
                DotNetJerryHost.WriteLine("  scaffold                    Generate a C# object model from an existing database.");
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
            DotNetJerryHost.WriteLine("  -v, --vendor <moniker>      Vendor used to connect to database. Moniker can be");
            DotNetJerryHost.WriteLine("                                  'sqlserver', 'sqlite', 'oracle', 'postgres' or");
            DotNetJerryHost.WriteLine("                                  'mysql'.");
            DotNetJerryHost.WriteLine("  -c, --connection <cs>       Connection string used to connect to database.");
            DotNetJerryHost.WriteLine("  -ns, --namespace <ns>       Namespace to place scaffolded C# classes in.");
            DotNetJerryHost.WriteLine("  -o, --output <file>         Path to scaffold .cs files into. Writes one");
            DotNetJerryHost.WriteLine("                                  file per class unless specified with .cs");
            DotNetJerryHost.WriteLine("                                  extension. Defaults to Database.cs.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  jerry scaffold -v sqlserver -c \"SERVER=...\" -ns MovieDb.Data.Database");
            DotNetJerryHost.WriteLine("  jerry scaffold -v sqlserver -c \"SERVER=...\" -o MyModel.cs");
            DotNetJerryHost.WriteLine("  jerry scaffold -v sqlserver -c \"SERVER=...\" -o Database");
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
            DotNetJerryHost.WriteLine("  -p, --project          Project directory to resolve relative paths from. Default to the");
            DotNetJerryHost.WriteLine("                             current directory.");
            DotNetJerryHost.WriteLine("  -f, --file             Add a file to the project. Prefix with @ to read from a file-based");
            DotNetJerryHost.WriteLine("                             list (.rsp syntax).");
            DotNetJerryHost.WriteLine("  -d, --directory        Add all .cssql files from a directory and its subdirectories");
            DotNetJerryHost.WriteLine("                             to the project.");
            DotNetJerryHost.WriteLine("  -ns, --namespace       Root namespace from which to generate sub-namespaces for each .cssql");
            DotNetJerryHost.WriteLine("                             file based on its relative project path.");
            DotNetJerryHost.WriteLine("  -i, --import           Add a default namespace import to each generated C# class.");
            DotNetJerryHost.WriteLine("  -o, --output           Specify the output directory for generated C# files. Defaults");
            DotNetJerryHost.WriteLine("                             to 'obj\\Jerrycurl'");
            DotNetJerryHost.WriteLine("  --no-clean             Do not clean the output directory from existing C# files before");
            DotNetJerryHost.WriteLine("                             transpiling.");
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine("Examples:");
            DotNetJerryHost.WriteLine("  jerry transpile -d . -ns MovieDb.Data -o MovieDb\\obj");
            DotNetJerryHost.WriteLine("  jerry transpile -f Query1.cssql Query2.cssql");
            DotNetJerryHost.WriteLine("  jerry transpile -f @FileList.txt");
            DotNetJerryHost.WriteLine("  jerry transpile -p MovieDb -d MovieDb");
            DotNetJerryHost.WriteLine("  jerry transpile -d . -i MovieDb.Model");
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
            DotNetJerryHost.WriteLine("  jerry info -v sqlserver");
            DotNetJerryHost.WriteLine();
        }

        public static void HelpForInvalid(RunnerArgs args)
        {
            DotNetJerryHost.WriteHeader();
            DotNetJerryHost.WriteLine("Usage: jerry [command] [options]. Use 'jerry help' to show commands and options.");
            DotNetJerryHost.WriteLine();

            if (string.IsNullOrEmpty(args.Command))
                throw new RunnerException("No command specified.");
            else
                throw new RunnerException($"Invalid command '{args.Command}'.");
        }
    }
}
