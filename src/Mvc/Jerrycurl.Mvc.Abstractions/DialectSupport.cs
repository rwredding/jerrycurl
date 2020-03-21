using System;

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
