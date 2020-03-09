using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Data;

namespace Jerrycurl.Data.Sessions
{
    public class Parameter : IParameter
    {
        public string Name { get; }
        public IField Field { get; }

        public Parameter(string name, IField field = null)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Field = field;
        }

        public void Build(IDbDataParameter adoParameter)
        {
            IBindingMetadata metadata = this.Field?.Identity.GetMetadata<IBindingMetadata>();
            IBindingParameterContract contract = metadata?.Parameter;

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

        public static Parameter Create<TValue>(ISchemaStore store, string parameterName, TValue value) => new Parameter(parameterName, Model.Create(store, value));
    }
}
