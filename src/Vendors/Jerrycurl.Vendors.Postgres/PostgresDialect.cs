using Jerrycurl.Mvc;

namespace Jerrycurl.Vendors.Postgres
{
    public class PostgresDialect : IsoDialect
    {
        protected override char IdentifierQuote => '"';
        protected override char? ParameterPrefix => ':';
        protected override char? VariablePrefix => null;
    }
}
