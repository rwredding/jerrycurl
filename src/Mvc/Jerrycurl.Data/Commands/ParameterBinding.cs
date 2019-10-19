using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
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
    }
}
