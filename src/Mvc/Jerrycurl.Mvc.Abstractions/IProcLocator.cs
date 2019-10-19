using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IProcLocator
    {
        PageDescriptor FindPage(string procName, Type originType);
    }
}
