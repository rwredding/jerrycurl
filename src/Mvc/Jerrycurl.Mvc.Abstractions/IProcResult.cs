using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IProcResult
    {
        ISqlBuffer Buffer { get; }
        IDomainOptions Domain { get; }
    }
}
