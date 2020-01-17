using System.Collections.Generic;

namespace Jerrycurl.Relations.Tests.Models
{
    public class TupleModel
    {
        public int Value { get; set; }
        public NestedModel Nested { get; set; }

        public IList<NestedList> NestedLists { get; set; }

        public IList<NestedModel> List { get; set; }

        public IList<NestedModel> List2 { get; set; }

        public class NestedModel
        {
            public string Name { get; set; }
        }

        public class NestedList
        {
            public string Name { get; set; }
            public IList<NestedModel> List { get; set; }
        }
    }
}
