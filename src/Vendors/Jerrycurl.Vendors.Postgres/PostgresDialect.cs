using Jerrycurl.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Vendors.Postgres
{
    public class PostgresDialect : IsoDialect
    {
        protected override char IdentifierQuote => '"';
        protected override char? ParameterPrefix => ':';
        protected override char? VariablePrefix => null;
    }
}
