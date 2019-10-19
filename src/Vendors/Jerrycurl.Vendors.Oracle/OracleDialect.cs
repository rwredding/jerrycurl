using Jerrycurl.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Vendors.Oracle
{
    public class OracleDialect : IsoDialect
    {
        protected override char IdentifierQuote => '"';
        protected override char? ParameterPrefix => ':';
        protected override char? StringPrefix => null;
    }
}
