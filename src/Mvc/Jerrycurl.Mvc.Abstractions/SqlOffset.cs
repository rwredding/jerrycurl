using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public struct SqlOffset
    {
        public int NumberOfParams { get; set; }
        public int NumberOfBindings { get; set; }
        public int Text { get; set; }
    }
}
