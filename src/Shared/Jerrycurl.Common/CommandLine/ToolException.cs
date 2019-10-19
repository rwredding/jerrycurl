using System;

namespace Jerrycurl.CommandLine
{
    internal class ToolException : Exception
    {
        public string StdErr { get; }
        public string StdOut { get; }

        public ToolException(string stdErr, string stdOut, Exception innerException = null)
            : base(innerException?.Message ?? "Error running tool.", innerException)
        {
            this.StdErr = stdErr;
            this.StdOut = stdOut;
        }
    }
}
