using Jerrycurl.CommandLine;
using Jerrycurl.Reflection;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Runners;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.DotNet.Cli
{
    public class DotNetJerryHost
    {
        public async static Task<int> Main(string[] args)
        {
            RunnerArgs runnerArgs;
            
            try
            {
                runnerArgs = RunnerArgs.FromCommandLine(args);
            }
            catch (FileNotFoundException ex)
            {
                WriteHeader();
                WriteLine();
                WriteLine($"Could not expand '@{Path.GetFileName(ex.FileName)}'. File not found.", ConsoleColor.Red);

                return -1;
            }
            catch (Exception ex)
            {
                WriteHeader();
                WriteLine();
                WriteLine(ex.Message, ConsoleColor.Red);

                return -1;
            }

            try
            {
                await new ProgramRunner(runnerArgs).ExecuteAsync();
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
                bool isVerbose = runnerArgs?.Verbose ?? false;

                WriteLine(isVerbose ? ex.ToString() : ex.Message, ConsoleColor.Red);

                return -1;
            }

            return 0;
        }

        public static void WriteHeader()
        {
            NuGetVersion version = RunnerArgs.GetNuGetPackageVersion();

            if (version == null)
                WriteLine($"Jerrycurl CLI");
            else if (version.CommitHash != null)
                WriteLine($"Jerrycurl CLI v{version.PublicVersion} ({version.CommitHash})");
            else
                WriteLine($"Jerrycurl CLI v{version.PublicVersion}");
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
