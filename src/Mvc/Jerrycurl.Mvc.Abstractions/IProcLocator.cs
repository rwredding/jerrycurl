using System;

namespace Jerrycurl.Mvc
{
    public interface IProcLocator
    {
        PageDescriptor FindPage(string procName, Type originType);
    }
}
