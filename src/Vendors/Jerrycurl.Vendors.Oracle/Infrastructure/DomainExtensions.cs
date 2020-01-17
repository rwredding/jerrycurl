using Jerrycurl.Vendors.Oracle;
using Jerrycurl.Vendors.Oracle.Filters;
using Jerrycurl.Vendors.Oracle.Metadata;
using Oracle.ManagedDataAccess.Client;

namespace Jerrycurl.Mvc
{
    public static class DomainExtensions
    {
        /// <summary>
        /// Configures the current domain to connect to a Oracle database with a specified connection string.
        /// </summary>
        /// <param name="options">A <see cref="DomainOptions"/> instance from the <see cref="IDomain.Configure(DomainOptions)"/> method.</param>
        /// <param name="connectionString">Connection string specifying the details of the connection.</param>
        public static void UseOracle(this DomainOptions options, string connectionString)
        {
            options.ConnectionFactory = () => new OracleConnection(connectionString);
            options.Schemas.AddContract(new OracleContractResolver());
            options.Sql.Filters.Add(new OracleLongFilter());
            options.Dialect = new OracleDialect();
        }
    }
}
