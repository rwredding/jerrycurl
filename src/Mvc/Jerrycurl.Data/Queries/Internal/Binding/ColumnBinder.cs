using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Parsing;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class ColumnBinder : ValueBinder
    {
        public ColumnBinder(Node node)
            : base(node)
        {

        }

        public ColumnMetadata Column { get; set; }
        public ParameterExpression Helper { get; set; }
    }
}
