using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Shared
{
    public class LocatorQuery3_cssql : ProcPage<object, object>
    {
        public LocatorQuery3_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
