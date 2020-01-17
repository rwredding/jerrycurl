using System.Collections.Generic;

namespace Jerrycurl.Mvc.Test.Conventions.Accessors
{
    public class MiscAccessor : Accessor
    {
        public IList<int> TemplatedQuery() => this.Query<int>();
        public IList<int> PartialedQuery() => this.Query<int>();
    }
}
