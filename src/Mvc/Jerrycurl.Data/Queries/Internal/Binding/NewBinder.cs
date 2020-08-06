using System.Collections.Generic;

namespace Jerrycurl.Data.Queries.Internal.Binding
{
    internal class NewBinder : NodeBinder
    {
        public KeyBinder PrimaryKey { get; set; }
        public IList<KeyBinder> JoinKeys { get; set; } = new List<KeyBinder>();
        public IList<NodeBinder> Properties { get; set; } = new List<NodeBinder>();
        public bool IsDynamic { get; set; }
    }
}
