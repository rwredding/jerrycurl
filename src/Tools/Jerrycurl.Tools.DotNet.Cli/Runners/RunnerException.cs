using System;
using System.Runtime.Serialization;

namespace Jerrycurl.Tools.DotNet.Cli.Runners
{
    internal class RunnerException : Exception
    {
        public string VerboseMessage { get; private set; }

        public RunnerException()
        {

        }

        public RunnerException(string message)
            : base(message)
        {

        }

        public RunnerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected RunnerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ToString() => this.VerboseMessage ?? base.ToString();
    }
}
