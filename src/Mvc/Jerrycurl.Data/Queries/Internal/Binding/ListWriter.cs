using System.Diagnostics;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    [DebuggerDisplay("{GetType().Name,nq}: {Metadata.Identity.Name,nq}")]
    internal class ListWriter
    {
        public ParameterExpression Slot { get; set; }
        public int BufferIndex { get; set; }
        public bool IsOneToMany { get; set; }
        public NodeBinder Item { get; set; }
        public KeyBinder PrimaryKey { get; set; }
        public KeyBinder JoinKey { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public int Priority { get; set; }
    }
}
