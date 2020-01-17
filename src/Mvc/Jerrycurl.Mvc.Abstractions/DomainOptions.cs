using System;
using System.Data;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public class DomainOptions : IDomainOptions
    {
        public Func<IDbConnection> ConnectionFactory { get; set; }
        public IDialect Dialect { get; set; }
        public ISchemaStore Schemas { get; set; }
        public IProcEngine Engine { get; set; }
        public IProcServices Services { get; set; }
        public SqlOptions Sql { get; set; }

        ISqlOptions IDomainOptions.Sql => this.Sql;
    }
}
