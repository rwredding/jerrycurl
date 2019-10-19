using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Test.Project.Models;

namespace Jerrycurl.Test.Project.Commands
{
    public class Command_cssql : ProcPage<IRunnable, object>
    {
        public Command_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute() => this.Model.Execute(this);
    }
}
