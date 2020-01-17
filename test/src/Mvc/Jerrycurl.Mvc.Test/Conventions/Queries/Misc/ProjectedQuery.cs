using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Misc
{
    public class ProjectedQuery_cssql : ProcPage<object, object>
    {
        [Inject]
        public IProjection<int> p1 { get; set; }

        public ProjectedQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            if (this.p1 != null)
                this.WriteLiteral("PROJEXISTS");
        }
    }
}
