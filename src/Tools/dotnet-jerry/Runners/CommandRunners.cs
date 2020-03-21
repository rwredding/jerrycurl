using System;
using System.Data.Common;
using System.Threading.Tasks;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.Info;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal static partial class CommandRunners
    { 
        public static void Info(RunnerArgs info, InfoCommand command)
        {
            if (command == null)
                throw new RunnerException("Invalid command object.");

            DotNetJerryHost.WriteHeader();
            DotNetJerryHost.WriteLine($"Package: {info.Proxy.PackageName}");
            DotNetJerryHost.WriteLine($"Connector: {command.Connector} v{command.ConnectorVersion}");
        }

        public static void Meow()
        {
            DotNetJerryHost.WriteLine();
            DotNetJerryHost.WriteLine(@"   |\---/|");
            DotNetJerryHost.WriteLine(@"   | o_o |");
            DotNetJerryHost.WriteLine(@"    \_^_/ ");
            DotNetJerryHost.WriteLine();
        }

        private async static Task<DbConnection> GetOpenConnectionAsync(RunnerArgs args, IConnectionFactory factory)
        {
            DbConnection connection = factory.GetDbConnection();

            if (connection == null)
                throw new RunnerException("Connection returned null.");

            try
            {
                connection.ConnectionString = args.Connection;
            }
            catch (Exception ex)
            {
                connection.Dispose();

                throw new RunnerException("Invalid connection string: " + ex.Message, ex);
            }

            if (!string.IsNullOrEmpty(connection.Database))
                DotNetJerryHost.WriteLine($"Connecting to '{connection.Database}'...", ConsoleColor.Yellow);
            else
                DotNetJerryHost.WriteLine("Connecting to database...", ConsoleColor.Yellow);

            try
            {
                await connection.OpenAsync().ConfigureAwait(false);

                return connection;
            }
            catch (Exception ex)
            {
                connection.Dispose();

                throw new RunnerException("Unable to open connection: " + ex.Message, ex);
            }
        }
    }
}
