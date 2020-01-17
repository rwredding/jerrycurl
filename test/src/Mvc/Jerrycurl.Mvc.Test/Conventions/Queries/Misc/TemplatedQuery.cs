using Jerrycurl.Mvc.Metadata.Annotations;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Misc
{
    [Template("TemplateQuery2.cssql")]
    public class TemplatedQuery_cssql : ProcPage<object, object>
    {
        public TemplatedQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            this.WriteLiteral("SELECT 3 AS `Item`");
        }
    }
}
