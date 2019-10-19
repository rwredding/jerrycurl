using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IProcExecutionStack
    {
        IPageExecutionContext Current { get; }

        void Push(IPageExecutionContext context);
        IPageExecutionContext Pop();
    }
}
