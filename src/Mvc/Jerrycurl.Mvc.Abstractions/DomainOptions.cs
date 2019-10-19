using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Relations.Metadata;
using System.Reflection;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Data.Filters;

namespace Jerrycurl.Mvc
{
    public class DomainOptions : IDomainOptions
    {
        public Func<IDbConnection> ConnectionFactory { get; set; }
        public IDialect Dialect { get; set; }
        public ISchemaStore Schemas { get; set; }
        public IProcEngine Engine { get; set; }
        public IServiceResolver Services { get; set; }
        public SqlOptions Sql { get; set; }

        ISqlOptions IDomainOptions.Sql => this.Sql;
    }
}
