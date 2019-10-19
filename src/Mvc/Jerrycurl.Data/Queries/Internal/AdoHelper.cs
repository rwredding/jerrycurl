using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class AdoHelper : IAdoCommandBuilder
    {
        public QueryData Data { get; }

        public AdoHelper(QueryData queryData)
        {
            this.Data = queryData ?? throw new ArgumentNullException(nameof(queryData));
        }

        public void Build(IDbCommand adoCommand)
        {
            adoCommand.CommandText = this.Data.QueryText;

            if (this.Data.Parameters != null)
            {
                foreach (IParameter parameter in this.Data.Parameters.GroupBy(p => p.Name).Select(g => g.First()))
                    adoCommand.Parameters.Add(ParameterHelper.CreateAdoParameter(adoCommand, parameter));
            }
        }
    }
}
