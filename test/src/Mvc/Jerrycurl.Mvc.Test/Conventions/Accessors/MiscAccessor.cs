using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc.Test.Conventions.Accessors
{
    public class MiscAccessor : Accessor
    {
        public IList<int> TemplatedQuery() => this.Query<int>();
        public IList<int> PartialedQuery() => this.Query<int>();
    }
}
