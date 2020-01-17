using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Test.Conventions.DependencyInjection.Services;

namespace Jerrycurl.Mvc.Test.Conventions.DependencyInjection.Queries
{
    public class DiQuery_cssql : ProcPage<object, object>
    {
        [Inject]
        public MyService ms { get; set; }

        [Inject]
        public IProjection<int> p1 { get; set; }

        public DiQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            this.WriteLiteral(this.ms.SomeValue);

            if (this.p1 != null)
                this.WriteLiteral("+PROJ");
        }
    }
}
