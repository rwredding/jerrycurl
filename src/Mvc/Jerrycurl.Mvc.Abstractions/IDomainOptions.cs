using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Filters;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public interface IDomainOptions
    {
        Func<IDbConnection> ConnectionFactory { get; }
        IDialect Dialect { get; }
        ISchemaStore Schemas { get; }
        IProcEngine Engine { get; }
        IServiceResolver Services { get; }
        ISqlOptions Sql { get; }
    }
}
