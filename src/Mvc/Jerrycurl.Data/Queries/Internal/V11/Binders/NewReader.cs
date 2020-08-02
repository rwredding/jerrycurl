using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class NewReader : NodeReader
    {
        public KeyReader PrimaryKey { get; set; }
        public IList<KeyReader> JoinKeys { get; set; }
        public IList<NodeReader> Properties { get; set; }
    }
}
