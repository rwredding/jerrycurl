using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class ColumnBinder : ValueBinder
    {
        public ColumnInfo Column { get; set; }
        public ParameterExpression Helper { get; set; }
    }
}
