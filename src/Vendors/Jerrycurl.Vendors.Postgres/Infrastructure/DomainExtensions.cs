using Jerrycurl.Vendors.Postgres;
using Jerrycurl.Vendors.Postgres.Metadata;
using Npgsql;

namespace Jerrycurl.Mvc
{
    public static class DomainExtensions
    {
        /// <summary>
        /// Configures the current domain to connect to a PostgreSQL database with a specified connection string.
        /// </summary>
        /// <param name="options">A <see cref="DomainOptions"/> instance from the <see cref="IDomain.Configure(DomainOptions)"/> method.</param>
        /// <param name="connectionString">Connection string specifying the details of the connection.</param>
        public static void UsePostgres(this DomainOptions options, string connectionString)
        {
            options.ConnectionFactory = () => new NpgsqlConnection(connectionString);
            options.Dialect = new PostgresDialect();
            options.Schemas.AddContract(new PostgresContractResolver());
        }
    }
}
