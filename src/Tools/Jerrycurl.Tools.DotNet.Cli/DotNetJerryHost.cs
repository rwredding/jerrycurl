using Jerrycurl.CommandLine;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Runners;
using System;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.DotNet.Cli
{
    public class DotNetJerryHost
    {
        public async static Task<int> Main(string[] args2)
        {
            RunnerArgs args = RunnerArgs.FromCommandLine(args2);

            ProgramRunner runner = new ProgramRunner(args);

            try
            {
                await runner.ExecuteAsync();
            }
            catch (ToolException ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.StdOut))
                    WriteLine(ex.StdOut, ConsoleColor.Red);

                if (!string.IsNullOrWhiteSpace(ex.StdErr))
                    WriteLine(ex.StdErr, ConsoleColor.Red);

                return -1;
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message, ConsoleColor.Red);

                return -1;
            }

            return 0;
        }

        public static void WriteHeader()
        {
            string nugetVersion = RunnerArgs.GetNuGetPackageVersion();
            string nugetHash = RunnerArgs.GetNuGetPackageHash();

            if (nugetVersion != null && nugetHash != null)
                Console.WriteLine($"Jerrycurl CLI v{nugetVersion} ({nugetHash})");
            else if (nugetVersion != null)
                Console.WriteLine($"Jerrycurl CLI v{nugetVersion}");
            else
                Console.WriteLine($"Jerrycurl CLI");
        }

        public static void WriteLine() => Console.WriteLine();
        public static void WriteLine(string message, ConsoleColor? color = null)
        {
            if (color != null)
                Console.ForegroundColor = color.Value;

            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
