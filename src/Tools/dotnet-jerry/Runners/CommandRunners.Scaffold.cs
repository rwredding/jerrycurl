using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Scaffolding;
using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal static partial class CommandRunners
    {
        public static async Task ScaffoldAsync(RunnerArgs args, ScaffoldCommand command)
        {
            if (command == null)
                throw new RunnerException("Invalid command object.");

            if (string.IsNullOrWhiteSpace(args.Connection))
                throw new RunnerException("Please specify a connection string using the -c|--connection parameter.");

            DatabaseModel databaseModel;
            IList<TypeMapping> typeMappings;

            if (args.Verbose)
                DotNetJerryHost.WriteHeader();

            using (DbConnection connection = await GetOpenConnectionAsync(args, command))
            {
                DotNetJerryHost.WriteLine("Generating...", ConsoleColor.Yellow);

                databaseModel = await command.GetDatabaseModelAsync(connection);
                typeMappings = command.GetTypeMappings().ToList();
            }

            ScaffoldProject project = ScaffoldProject.FromModel(databaseModel, typeMappings, args);

            await ScaffoldWriter.WriteAsync(project);

            if (args.Verbose)
                ScaffoldWriteVerboseOutput(project);

            int objectCount = project.Files.SelectMany(f => f.Objects).Count();

            string classMoniker = objectCount + " " + (objectCount == 1 ? "class" : "classes");

            if (project.Files.Count == 1)
                DotNetJerryHost.WriteLine($"Created {classMoniker} in {project.Files[0].FileName}.", ConsoleColor.Green);
            else
                DotNetJerryHost.WriteLine($"Created {classMoniker} in {project.Files.Count} files.", ConsoleColor.Green);
        }

        private static void ScaffoldWriteVerboseOutput(ScaffoldProject project)
        {
            foreach (var file in project.Files)
            {
                DotNetJerryHost.WriteLine($"    File {file.FileName}");

                foreach (var obj in file.Objects)
                {
                    string tableName = !string.IsNullOrEmpty(obj.Table.Schema) ? $"\"{obj.Table.Schema}\".\"{obj.Table.Name}\"" : $"\"{obj.Table.Name}\"";
                    string className = !string.IsNullOrEmpty(obj.Namespace) ? $"{obj.Namespace}.{obj.ClassName}" : obj.ClassName;

                    DotNetJerryHost.WriteLine($"        -> Table {tableName} -> Class {className}");

                    string[] columnNames = obj.Properties.Select(p => $"\"{p.PropertyName}\"").ToArray();
                    string columnMoniker = string.Join(", ", columnNames.Take(5));

                    if (columnNames.Length > 5)
                        columnMoniker += $" [+{columnNames.Length - 5}]";

                    DotNetJerryHost.WriteLine($"            -> Property {columnMoniker}");
                }                
            }
        }
    }
}
