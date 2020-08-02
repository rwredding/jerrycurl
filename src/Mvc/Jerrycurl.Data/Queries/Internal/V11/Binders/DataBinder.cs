using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class DataBinder : ValueBinder
    {
        public ColumnIdentity Column { get; set; }
        public ParameterExpression Helper { get; set; }
    }
}
