using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Data
{
    internal static class ParameterHelper
    {
        public static IDbDataParameter CreateAdoParameter(IDbCommand adoCommand, IParameter parameter)
        {
            IDbDataParameter adoParameter = adoCommand.CreateParameter();

            IBindingMetadata metadata = parameter.Field?.Identity.GetMetadata<IBindingMetadata>();
            IBindingParameterContract contract = parameter.Contract ?? metadata?.Parameter;

            adoParameter.ParameterName = parameter.Name;

            if (contract?.Convert != null)
                adoParameter.Value = contract.Convert(parameter.Field?.Value);
            else if (parameter.Field != null)
                adoParameter.Value = parameter.Field.Value;
            else
                adoParameter.Value = DBNull.Value;

            if (contract?.Write != null)
            {
                BindingParameterInfo paramInfo = new BindingParameterInfo()
                {
                    Metadata = metadata,
                    Parameter = adoParameter,
                    Field = parameter.Field,
                };

                contract.Write(paramInfo);
            }

            return adoParameter;
        }
    }
}
