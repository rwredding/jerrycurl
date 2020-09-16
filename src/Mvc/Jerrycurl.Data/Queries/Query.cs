using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Queries
{
    public class Query : IBatch
    {
        public string QueryText { get; set; }
        public ICollection<IParameter> Parameters { get; set; } = new List<IParameter>();

        public void Build(IDbCommand adoCommand)
        {
            adoCommand.CommandText = this.QueryText;

            if (this.Parameters != null)
            {
                foreach (IParameter parameter in this.Parameters.GroupBy(p => p.Name).Select(g => g.First()))
                {
                    IDbDataParameter adoParameter = adoCommand.CreateParameter();

                    parameter.Build(adoParameter);

                    adoCommand.Parameters.Add(adoParameter);
                }
            }
        }
    }
}
