using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Misc
{
    public class InjectedQuery_cssql : ProcPage<object, object>
    {
        [Inject]
        public DependencyInjection.Services.MyService ms { get; set; }

        public InjectedQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {

        }
    }
}
