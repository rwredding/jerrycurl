using System.Linq.Expressions;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    internal class DataBinder : ValueBinder
    {
        public ColumnIdentity Column { get; set; }
        public ParameterExpression Helper { get; set; }
    }
}
