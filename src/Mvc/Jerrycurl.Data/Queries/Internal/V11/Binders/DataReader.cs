using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class DataReader : NodeReader
    {
        public ColumnIdentity Column { get; set; }
        public ParameterExpression Helper { get; set; }
        public ParameterExpression IsDbNull { get; set; }
        public ParameterExpression Value { get; set; }
        public bool CanBeDbNull { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsJoinKey { get; set; }
    }
}
