using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Commands.Shared
{
    public class LocatorCommand2_cssql : ProcPage<object, object>
    {
        public LocatorCommand2_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
