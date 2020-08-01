using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class EnumerateTree
    {
        public ISchema Schema { get; set; }
        public IList<HelperWriter> Helpers { get; set; }
        public NodeReader Item { get; set; }
    }
}
