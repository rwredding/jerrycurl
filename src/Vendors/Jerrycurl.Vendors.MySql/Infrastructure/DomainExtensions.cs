using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;
using MySql.Data.MySqlClient;
using Jerrycurl.Vendors.MySql;

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
