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

        public void Build(IDbDataParameter adoParameter)
        {
            IBindingMetadata metadata = this.Field?.Identity.GetMetadata<IBindingMetadata>();
            IBindingParameterContract contract = this.Contract ?? metadata?.Parameter;

            adoParameter.ParameterName = this.Name;

            if (contract?.Convert != null)
                adoParameter.Value = contract.Convert(this.Field?.Value);
            else if (this.Field != null)
                adoParameter.Value = this.Field.Value;
            else
                adoParameter.Value = DBNull.Value;

            if (contract?.Write != null)
            {
                BindingParameterInfo paramInfo = new BindingParameterInfo()
                {
                    Metadata = metadata,
                    Parameter = adoParameter,
                    Field = this.Field,
                };

                contract.Write(paramInfo);
            }
        }

        public override string ToString() => this.Name;
    }
}
