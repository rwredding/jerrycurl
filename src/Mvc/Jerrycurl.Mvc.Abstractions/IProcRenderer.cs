using Jerrycurl.Mvc.Projections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IProcRenderer
    {
        ISqlContent Body();
        ISqlContent Partial(string procName, IProjection model, IProjection result);
    }
}
