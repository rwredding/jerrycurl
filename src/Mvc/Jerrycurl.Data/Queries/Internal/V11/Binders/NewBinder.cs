using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11.Binders
{
    internal class NewBinder : NodeBinder
    {
        public ValueKey PrimaryKey { get; set; }
        public IList<ValueKey> JoinKeys { get; set; }
        public IList<NodeBinder> Properties { get; set; }
    }
}
