using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Test.Conventions.Models;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Crud
{
    public class Run_cssql : ProcPage<Runnable, object>
    {
        public Run_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute() => this.Model.Execute(this);
    }
}
