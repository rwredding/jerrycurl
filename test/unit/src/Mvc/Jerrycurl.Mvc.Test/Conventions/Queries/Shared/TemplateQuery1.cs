using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Shared
{
    public class TemplateQuery1_cssql : ProcPage<object, object>
    {
        public TemplateQuery1_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            this.WriteLiteral("SELECT 1 AS `Item` UNION\r\n");
            this.Write(this.Render.Body());
        }
    }
}
