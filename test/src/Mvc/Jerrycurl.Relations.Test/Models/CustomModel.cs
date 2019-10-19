using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Test.Models
{
    public class CustomModel
    {
        public CustomList<int> Values { get; set; }

        public List<int> List1 { get; set; }
        public List<int> List2 { get; set; }
        public List<int> List3 { get; set; }
    }
}
