using System.Diagnostics;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    [DebuggerDisplay("{GetType().Name,nq}: {Metadata.Identity.ToString(),nq}")]
    internal abstract class NodeBinder
    {
        public IBindingMetadata Metadata { get; set; }
    }
}
