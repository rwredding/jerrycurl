using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Locator
{
    public class LocatorQuery2_cssql : ProcPage<object, object>
    {
        public LocatorQuery2_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
