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
    }
}
