using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.CommandLine
{
    internal static class ToolRunner
    {
        public static Task RunAsync(string toolName, IEnumerable<string> toolArgs, string workingDirectory = null, bool capture = true)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolName,
                Arguments = ToolOptions.Escape(toolArgs),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = capture,
                RedirectStandardError = capture,
                RedirectStandardOutput = capture,
            };

            return RunAsync(startInfo);
        }

        public static async Task RunAsync(ProcessStartInfo startInfo, int timeout = 30000)
        {
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                StringBuilder outputBuilder = new StringBuilder();
                TaskCompletionSource<bool> outputCloseEvent = new TaskCompletionSource<bool>();

                if (startInfo.RedirectStandardOutput)
                {
                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                            outputCloseEvent.SetResult(true);
                        else
                            outputBuilder.AppendLine(e.Data);
                    };
                }
                else
                    outputCloseEvent.SetResult(true);


                StringBuilder errorBuilder = new StringBuilder();
                TaskCompletionSource<bool> errorCloseEvent = new TaskCompletionSource<bool>();

                if (startInfo.RedirectStandardError)
                {
                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                            errorCloseEvent.SetResult(true);
                        else
                            errorBuilder.AppendLine(e.Data);
                    };
                }
                else
                    errorCloseEvent.SetResult(true);

                try
                {
                    if (!process.Start())
                        throw new ToolException(errorBuilder.ToString(), outputBuilder.ToString());
                }
                catch (Exception ex)
                {
                    throw new ToolException(errorBuilder.ToString(), outputBuilder.ToString(), innerException: ex);
                }

                if (startInfo.RedirectStandardOutput)
                    process.BeginOutputReadLine();

                if (startInfo.RedirectStandardError)
                    process.BeginErrorReadLine();

                Task<bool> waitForExit = WaitForExitAsync(process, timeout);
                Task processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);

                if (await Task.WhenAny(Task.Delay(timeout), processTask) == processTask && await waitForExit)
                {
                    if (process.ExitCode != 0)
                        throw new ToolException(errorBuilder.ToString(), outputBuilder.ToString());
                }
                else
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }

                    throw new Exception("Timed out.");
                }
            }
        }


        private static Task<bool> WaitForExitAsync(Process process, int timeout) => Task.Run(() => process.WaitForExit(timeout));
    }
}
