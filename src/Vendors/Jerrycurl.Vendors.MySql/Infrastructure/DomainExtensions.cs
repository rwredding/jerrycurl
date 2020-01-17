using MySql.Data.MySqlClient;
using Jerrycurl.Vendors.MySql;
using Jerrycurl.Vendors.MySql.Metadata;

namespace Jerrycurl.Mvc
{
    public static class DomainExtensions
    {
        /// <summary>
        /// Configures the current domain to connect to a MySQL database with a specified connection string.
        /// </summary>
        /// <param name="options">A <see cref="DomainOptions"/> instance from the <see cref="IDomain.Configure(DomainOptions)"/> method.</param>
        /// <param name="connectionString">Connection string specifying the details of the connection.</param>
        public static void UseMySql(this DomainOptions options, string connectionString)
        {
            options.ConnectionFactory = () => new MySqlConnection(connectionString);
            options.Schemas.AddContract(new MySqlContractResolver());
            options.Dialect = new MySqlDialect();
        }
    }
}
