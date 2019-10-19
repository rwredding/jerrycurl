using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Jerrycurl.Mvc;
using Jerrycurl.Test;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class SqlServerConvention : DatabaseConvention
    {
        public override bool Skip => string.IsNullOrEmpty(this.GetConnectionString());
        public override string SkipReason => "Please configure connection in the 'JERRY_SQLSERVER_CONN' environment variable.";

        public override void Configure(DomainOptions options)
        {
            options.UseSqlServer(this.GetConnectionString());
            options.UseNewtonsoftJson();
        }

        private string GetConnectionString() => this.GetEnvironmentVariable("JERRY_SQLSERVER_CONN");
    }
}
