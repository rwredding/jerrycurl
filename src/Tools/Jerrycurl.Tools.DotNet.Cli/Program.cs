using Jerrycurl.CommandLine;
using Jerrycurl.Reflection;
using Jerrycurl.Tools.DotNet.Cli.Commands;
using Jerrycurl.Tools.DotNet.Cli.Runners;
using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.DotNet.Cli
{
    public class Program
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
            string packageVersion = RunnerArgs.GetNuGetPackageVersion();

            Console.WriteLine($"Jerrycurl CLI v{packageVersion}");
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
