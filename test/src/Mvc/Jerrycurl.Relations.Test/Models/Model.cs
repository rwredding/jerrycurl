using System.Collections.Generic;

namespace Jerrycurl.Relations.Tests.Models
{
    public class Model
    {
        public int IntValue { get; set; }
        public IEnumerable<int> IntEnumerable { get; set; }
        public List<int> IntList { get; set; }

        public List<SubModel> ComplexList { get; set; }
        public List<SubModel> ComplexList2 { get; set; }
        public SubModel Complex { get; set; }
        public object Object { get; set; }
        public int ReadOnly { get; }

        public class SubModel
        {
            public int Value { get; set; }
            public SubModel2 Complex { get; set; }
        }

        public class SubModel2
        {
            public string Value { get; set; }
        }
    }
}
