using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.CommandLine;
using Jerrycurl.IO;
using Jerrycurl.Tools.DotNet.Cli.Commands;

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

                        await ExecuteSqlAsync(connection, sqlText);

                        numberOfInputs++;
                    }
                    else if (IsRawInput(option))
                    {
                        string sqlText = string.Join("", option.Values.Select(File.ReadAllText));

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
                        if (args.Verbose)
                        {
                            DotNetJerryHost.WriteLine($"Executing...", ConsoleColor.Yellow);
                            DotNetJerryHost.WriteLine(sqlText, ConsoleColor.Blue);
                        }
                        else
                            DotNetJerryHost.WriteLine($"Executing '{GetSqlPreviewText(sqlText)}'...", ConsoleColor.Yellow);

                        int affectedRows = await command.ExecuteNonQueryAsync();

                        string rowsMoniker = affectedRows + " " + (affectedRows == 1 ? "row" : "rows");

                        DotNetJerryHost.WriteLine($"OK. {rowsMoniker} affected.", ConsoleColor.Green);
                    }
                    else
                        DotNetJerryHost.WriteLine($"Skipped. SQL text is empty.", ConsoleColor.Yellow);
                }
            }

            string GetSqlPreviewText(string sqlText)
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < sqlText.Length && builder.Length <= 30; i++)
                {
                    if (!char.IsWhiteSpace(sqlText[i]))
                        builder.Append(sqlText[i]);
                    else if (builder.Length > 0 && !char.IsWhiteSpace(builder[builder.Length - 1]))
                        builder.Append(' ');
                }

                return builder.ToString();
            }

            bool IsRawInput(ToolOption option) => (option.Name == "raw" || option.ShortName == "r");
            bool IsSqlInput(ToolOption option) => (option.Name == "sql" || option.ShortName == "s");
            bool IsFileInput(ToolOption option) => (option.Name == "file" || option.ShortName == "f");
        }
    }
}
