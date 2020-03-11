using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Generation;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Scaffolding;
using Jerrycurl.Tools.Info;
using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal static partial class CommandRunners
    {
        public static async Task ScaffoldAsync(RunnerArgs info, ScaffoldCommand command)
        {
            if (command == null)
                throw new RunnerException("Invalid command object.");

            if (string.IsNullOrWhiteSpace(info.Connection))
                throw new RunnerException("Please specify a connection string using the -c|--connection parameter.");

            DatabaseModel databaseModel;
            IList<TypeMapping> typeMappings;

            using (DbConnection connection = command.GetDbConnection())
            {
                try
                {
                    connection.ConnectionString = info.Connection;
                }
                catch (Exception ex)
                {
                    throw new RunnerException("Invalid connection string: " + ex.Message, ex);
                }

                if (!string.IsNullOrEmpty(connection.Database))
                    Program.WriteLine($"Connecting to database '{connection.Database}'...", ConsoleColor.Yellow);
                else
                    Program.WriteLine("Connecting to database...", ConsoleColor.Yellow);

                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    throw new RunnerException("Unable to open connection: " + ex.Message, ex);
                }

                Program.WriteLine("Generating...", ConsoleColor.Yellow);

                databaseModel = await command.GetDatabaseModelAsync(connection);
                typeMappings = command.GetTypeMappings().ToList();
            }

            ScaffoldProject project = ScaffoldProject.FromModel(databaseModel, typeMappings, info);

            await ScaffoldWriter.WriteAsync(project);

            int tableCount = project.Files.SelectMany(f => f.Objects).Count();
            int columnCount = project.Files.SelectMany(f => f.Objects).SelectMany(o => o.Properties).Count();

            string tablesMoniker = tableCount + " " + (tableCount == 1 ? "table" : "tables");
            string columnsMoniker = columnCount + " " + (columnCount == 1 ? "column" : "columns");

            Console.ForegroundColor = ConsoleColor.Green;

            if (project.Files.Count == 1)
                Program.WriteLine($"Generated {tablesMoniker} and {columnsMoniker} in {project.Files[0].FileName}.", ConsoleColor.Green);
            else
                Program.WriteLine($"Generated {tablesMoniker} and {columnsMoniker} in {project.Files.Count} files.", ConsoleColor.Green);

            Console.ResetColor();
        }

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

        public static void Transpile(RunnerArgs args)
        {
            RazorProject project = new RazorProject()
            {
                RootNamespace = args.Options["-ns", "--namespace"]?.Value ?? "Jerrycurl.Procedures",
                Items = new List<RazorProjectItem>(),
                ProjectDirectory = args.Options["-cwd"]?.Value ?? Environment.CurrentDirectory,
            };

            if (args.Options["-f", "--file"] != null)
            {
                foreach (string file in args.Options["-f", "--file"].Values)
                    project.Items.Add(new RazorProjectItem() { ProjectPath = file });
            }

            if (args.Options["-d", "--directory"] != null)
            {
                foreach (string dir in args.Options["-d", "--directory"].Values)
                {
                    RazorProject fromDir = RazorProject.FromDirectory(dir);

                    foreach (RazorProjectItem item in fromDir.Items)
                        project.Items.Add(item);
                }
            }

            string sourcePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            GeneratorOptions options = new GeneratorOptions()
            {
                TemplateCode = File.ReadAllText(Path.Combine(sourcePath, "skeleton.jerry")),
            };

            if (args.Options["-i", "--import"] != null)
            {
                foreach (string import in args.Options["-i", "--import"].Values)
                    options.Imports.Add(new RazorFragment() { Text = import });
            }

            string outputDir = args.Options["-o", "--output"]?.Value ?? Path.Combine(Environment.CurrentDirectory, "obj", "Jerrycurl");

            Directory.CreateDirectory(outputDir);

            RazorParser parser = new RazorParser();
            RazorGenerator generator = new RazorGenerator(options);

            Program.WriteLine("Parsing...", ConsoleColor.Yellow);
            IList<RazorPage> parserResult = parser.Parse(project).ToList();

            Program.WriteLine("Transpiling...", ConsoleColor.Yellow);
            foreach (RazorPage razorPage in parserResult)
            {
                ProjectionResult result = generator.Generate(razorPage.Data);

                string baseName = Path.GetFileNameWithoutExtension(razorPage.ProjectPath ?? razorPage.Path);
                string fileName = $"{baseName}.{razorPage.Path.GetHashCode():x2}.g.cssql.cs";
                string fullName = Path.Combine(outputDir, fileName);

                File.WriteAllText(fullName, result.Content);
            }

            string filesMoniker = project.Items.Count + " " + (project.Items.Count == 1 ? "file" : "files");

            Console.ForegroundColor = ConsoleColor.Green;

            Program.WriteLine($"Transpiled {filesMoniker} into '{outputDir}'");

            Console.ResetColor();
        }

        public static void Help(RunnerArgs args)
        {
            if (args.Command == "help")
            {
                Program.WriteHeader();

                Program.WriteLine("Usage: jerry [command] [options]");
                Program.WriteLine();
                Program.WriteLine("Commands:");
                Program.WriteLine("    scaffold                    Generate C# classes from an existing database.");
                Program.WriteLine("    info                        Show information about a specific vendor.");
                Program.WriteLine("    help                        Show this information.");
                Program.WriteLine();
                Program.WriteLine("Options:");
                Program.WriteLine("    -v, --vendor <moniker>      Vendor used to connect to database. Moniker can");
                Program.WriteLine("                                    be 'sqlserver', 'sqlite', 'oracle', 'postgres'");
                Program.WriteLine("                                    or 'mysql'.");
                Program.WriteLine("    -c, --connection <cs>       Connection string used to connect to database.");
                Program.WriteLine("    -ns, --namespace <ns>       Namespace to place scaffolded C# classes in.");
                Program.WriteLine("    -o, --output <file>         Path to scaffold .cs files into. Writes one");
                Program.WriteLine("                                    file per class unless specified with .cs");
                Program.WriteLine("                                    extension. Defaults to Database.cs.");
                //Program.WriteLine("    --verbose                   Show verbose output.");
                Program.WriteLine();
            }
            else
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
}
