using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data
{
    public class Parameter : IParameter
    {
        public string Name { get; }
        public IField Field { get; }
        public IBindingParameterContract Contract { get; }

        public Parameter(string name, IField field = null, IBindingParameterContract contract = null)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Field = field;
            this.Contract = contract;
        }

        public override string ToString()
        {
            return this.Name + " = " + (this.Field?.Value?.ToString() ?? "NULL");
        }
    }
}
