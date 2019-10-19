using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Test.Conventions.Models;

namespace Jerrycurl.Mvc.Test.Conventions.Queries
{
    public class RunnableQuery_cssql : ProcPage<Runnable, object>
    {
        public RunnableQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute() => this.Model.Execute(this);
    }
}
