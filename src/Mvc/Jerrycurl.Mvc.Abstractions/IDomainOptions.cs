using System;
using System.Data;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public interface IDomainOptions
    {
        Func<IDbConnection> ConnectionFactory { get; }
        IDialect Dialect { get; }
        ISchemaStore Schemas { get; }
        IProcEngine Engine { get; }
        IProcServices Services { get; }
        ISqlOptions Sql { get; }
    }
}
