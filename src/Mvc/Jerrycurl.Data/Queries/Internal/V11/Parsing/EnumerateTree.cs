using System.Collections.Generic;
using Jerrycurl.Data.Queries.Internal.V11.Binding;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class EnumerateTree
    {
        public ISchema Schema { get; set; }
        public IList<HelperWriter> Helpers { get; } = new List<HelperWriter>();
        public NodeBinder Item { get; set; }
    }
}
