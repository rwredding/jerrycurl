using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Commands
{
    public class LocatorCommand3_cssql : ProcPage<object, object>
    {
        public LocatorCommand3_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
