using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Mvc;
using Jerrycurl.Test;

namespace Jerrycurl.Vendors.MySql.Test
{
    public class MySqlConvention : DatabaseConvention
    {
        public override bool Skip => string.IsNullOrEmpty(this.GetConnectionString());
        public override string SkipReason => "Please configure connection in the 'JERRY_MYSQL_CONN' environment variable.";

        public override void Configure(DomainOptions options)
        {
            options.UseMySql(this.GetConnectionString());
            options.UseNewtonsoftJson();
        }

        private string GetConnectionString() => this.GetEnvironmentVariable("JERRY_MYSQL_CONN");
    }
}
