using System.Linq.Expressions;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class JoinBinder : NodeBinder
    {
        public ParameterExpression Array { get; set; }
        public int ArrayIndex { get; set; }
        public bool IsManyToOne { get; set; }
    }
}
