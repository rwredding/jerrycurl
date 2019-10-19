using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Test.Models
{
    public struct BigStruct
    {
        public int Integer { get; set; }
        public string String { get; set; }
        public MySubStruct Sub { get; set; }

        public struct MySubStruct
        {
            public int Value { get; set; }
        }
    }
}
