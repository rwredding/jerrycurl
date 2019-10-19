using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions2.NoDomain.Queries
{
    public class NoDomainQuery_cssql : ProcPage<object, object>
    {
        public NoDomainQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }
    }
}
