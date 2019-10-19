using Jerrycurl.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Vendors.MySql
{
    public class MySqlDialect : IsoDialect
    {
        protected override char IdentifierQuote => '`';
    }
}
