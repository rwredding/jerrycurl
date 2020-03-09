using System;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public class ParameterBinding : ICommandBinding
    {
        public string ParameterName { get; }
        public IField Field { get; }

        public ParameterBinding(string parameterName, IField field)
        {
            this.ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            this.Field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public ParameterBinding(IParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            this.ParameterName = parameter.Name;
            this.Field = parameter.Field;
        }

        public override string ToString() => this.ParameterName;
    }
}
