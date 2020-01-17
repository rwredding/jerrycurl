using Jerrycurl.Data.Sessions;
using System;
using System.Data;
using System.Linq;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class Query : IOperation
    {
        public QueryData Data { get; }
        public object Source => this.Data;

        public Query(QueryData queryData)
        {
            this.Data = queryData ?? throw new ArgumentNullException(nameof(queryData));
        }

        public void Build(IDbCommand adoCommand)
        {
            adoCommand.CommandText = this.Data.QueryText;

            if (this.Data.Parameters != null)
            {
                foreach (IParameter parameter in this.Data.Parameters.GroupBy(p => p.Name).Select(g => g.First()))
                {
                    IDbDataParameter adoParameter = adoCommand.CreateParameter();

                    parameter.Build(adoParameter);

                    adoCommand.Parameters.Add(adoParameter);
                }
            }
        }
    }
}
