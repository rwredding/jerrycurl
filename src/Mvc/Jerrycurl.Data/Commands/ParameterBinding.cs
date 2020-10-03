using System;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public class ParameterBinding : IUpdateBinding
    {
        public string ParameterName { get; }
        public IField Target { get; }

        public ParameterBinding(string parameterName, IField target)
        {
            this.ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public ParameterBinding(IParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            this.ParameterName = parameter.Name;
            this.Target = parameter.Field;
        }

        public override string ToString() => $"ParameterBinding: {this.ParameterName} -> {this.Target.Identity.Name}";
    }
}
