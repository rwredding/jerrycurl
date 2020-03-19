using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Mvc;

namespace Jerrycurl.Test.Integration
{
    internal static class Settings
    {
        public static string ConnectionString { get; set; }

        public static void InitializeDomain(DomainOptions options)
        {
#if VENDOR_SQLSERVER
            options.UseSqlServer(ConnectionString);
#elif VENDOR_POSTGRES
            options.UsePostgres(ConnectionString);
            //options.Sql.Filters.Add(new InputParameterFilter());
#elif VENDOR_SQLITE
            options.UseSqlite(ConnectionString);
#elif VENDOR_ORACLE
            options.UseOracle(ConnectionString);
#elif VENDOR_MYSQL
            options.UseMySql(ConnectionString);
#endif
        }
    }
}
