using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Test.Models
{
    public class DeepModel
    {
        public SubModel1 Sub1 { get; set; }

        public class SubModel1
        {
            public SubModel2 Sub2 { get; set; }
        }

        public class SubModel2
        {
            public IList<SubModel3> Sub3 { get; set; }
        }

        public class SubModel3
        {
            public SubModel4 Sub4 { get; set; }
        }

        public class SubModel4
        {
            public IList<SubModel5> Sub5 { get; set; }
        }

        public class SubModel5
        {
            public IList<SubModel6> Sub6 { get; set; }
        }

        public class SubModel6
        {
            public int? Value { get; set; }
        }
    }
}
