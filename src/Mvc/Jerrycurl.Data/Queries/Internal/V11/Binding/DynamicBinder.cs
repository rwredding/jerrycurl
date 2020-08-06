using System.Collections.Generic;

namespace Jerrycurl.Data.Queries.Internal.V11.Binding
{
    internal class DynamicBinder : NodeBinder
    {
        public IList<NodeBinder> Properties { get; set; } = new List<NodeBinder>();
    }
}
