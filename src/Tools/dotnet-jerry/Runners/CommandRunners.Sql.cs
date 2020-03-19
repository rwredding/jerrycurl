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
        public async static Task SqlAsync(RunnerArgs args, IConnectionFactory factory)
        {
            if (factory == null)
                throw new RunnerException("Invalid factory object.");

            if (string.IsNullOrWhiteSpace(args.Connection))
                throw new RunnerException("Please specify a connection string using the -c|--connection argument.");

            int numberOfInputs = 0;

            using (DbConnection connection = await GetOpenConnectionAsync(args, factory))
            {
                foreach (ToolOption option in args.Options)
                {
                    if (IsSqlInput(option))
                    {
                        string sqlText = string.Join("\r\n", option.Values);

                        DotNetJerryHost.WriteLine($"Executing...", ConsoleColor.Yellow);
                        DotNetJerryHost.WriteLine(sqlText, ConsoleColor.Blue);

                        await ExecuteSqlAsync(connection, sqlText);

                        numberOfInputs++;
                    }
                    else if (IsFileInput(option))
                    {
                        ResponseSettings settings = new ResponseSettings()
                        {
                            IgnoreWhitespace = false,
                        };
                        string[] expanded = ResponseFile.ExpandFiles(option.Values, settings).ToArray();
                        string sqlText = string.Join("\r\n", expanded);

                        DotNetJerryHost.WriteLine($"Executing...", ConsoleColor.Yellow);
                        DotNetJerryHost.WriteLine(sqlText, ConsoleColor.Blue);

                        await ExecuteSqlAsync(connection, sqlText);

                        numberOfInputs++;
                    }
                    else if (IsRawInput(option))
                    {
                        string sqlText = string.Join("", option.Values.Select(File.ReadAllText));

                        DotNetJerryHost.WriteLine($"Executing...", ConsoleColor.Yellow);
                        DotNetJerryHost.WriteLine(sqlText, ConsoleColor.Blue);

                        await ExecuteSqlAsync(connection, sqlText);

                        numberOfInputs++;
                    }
                }
            }

            if (numberOfInputs == 0)
                throw new RunnerException("Please specify at least one SQL input with the --sql, --file or --raw arguments.");

            async Task ExecuteSqlAsync(DbConnection connection, string sqlText)
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqlText;

                    if (!string.IsNullOrWhiteSpace(command.CommandText))
                    {
                        int affectedRows = await command.ExecuteNonQueryAsync();

                        string rowsMoniker = affectedRows + " " + (affectedRows == 1 ? "row" : "rows");

                        DotNetJerryHost.WriteLine($"OK. {rowsMoniker} affected.", ConsoleColor.Green);
                    }
                    else
                        DotNetJerryHost.WriteLine($"Skipped. SQL text is empty.", ConsoleColor.Yellow);
                }
            }

            bool IsRawInput(ToolOption option) => (option.Name == "raw" || option.ShortName == "r");
            bool IsSqlInput(ToolOption option) => (option.Name == "sql" || option.ShortName == "s");
            bool IsFileInput(ToolOption option) => (option.Name == "file" || option.ShortName == "f");
        }
    }
}
