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

            string[] inputs = args.Options["-s", "--sql"]?.Values ?? Array.Empty<string>();

            if (inputs.Length == 0)
                throw new RunnerException("Please specify at least one SQL input with the -s|--sql argument.");

            using (DbConnection connection = await GetOpenConnectionAsync(args, factory))
            {
                foreach (ResponseFile responseFile in inputs.Select(ResponseFile.Parse))
                {
                    if (responseFile.IsPath)
                    {
                        DotNetJerryHost.WriteLine($"Executing '@{Path.GetFileName(responseFile.InputPath)}'...", ConsoleColor.Yellow);

                        if (!File.Exists(responseFile.FullPath))
                        {
                            DotNetJerryHost.WriteLine($"Skipped. File not found.", ConsoleColor.Yellow);

                            continue;
                        }
                    }
                    else if (!responseFile.Ignore)
                        DotNetJerryHost.WriteLine($"Executing '{responseFile.Value}'...", ConsoleColor.Yellow);

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Join(Environment.NewLine, ResponseFile.ExpandStrings(responseFile));

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
            }
        }
    }
}
