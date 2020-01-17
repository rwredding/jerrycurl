using Jerrycurl.Mvc;

namespace Jerrycurl.Vendors.MySql
{
    public class MySqlDialect : IsoDialect
    {
        protected override char IdentifierQuote => '`';
    }
}
