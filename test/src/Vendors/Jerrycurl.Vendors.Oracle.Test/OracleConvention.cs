using Jerrycurl.Mvc;
using Jerrycurl.Test;

namespace Jerrycurl.Vendors.Oracle.Test
{
    public class OracleConvention : DatabaseConvention
    {
        public override bool Skip => string.IsNullOrEmpty(this.GetConnectionString());
        public override string SkipReason => "Please configure connection in the 'JERRY_ORACLE_CONN' environment variable.";

        public override void Configure(DomainOptions options)
        {
            options.UseOracle(this.GetConnectionString());
            options.UseNewtonsoftJson();
        }

        public string GetConnectionString() => GetEnvironmentVariable("JERRY_ORACLE_CONN");
    }
}
