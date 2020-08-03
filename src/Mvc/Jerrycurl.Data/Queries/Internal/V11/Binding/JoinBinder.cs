using System.Linq.Expressions;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    internal class JoinBinder : NodeBinder
    {
        public ParameterExpression Array { get; set; }
        public int BufferIndex { get; set; }
        public bool IsOneValue { get; set; }
    }
}
