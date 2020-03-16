using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    [Flags]
    public enum DialectSupport
    {
        None = 0,
        InputParameters = 1,
        OutputParameters = 2,
    }
}
