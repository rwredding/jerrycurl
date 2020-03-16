using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Misc
{
    public class SubQuery_cssql : ProcPage<object, object>
    {
        public SubQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            this.WriteLiteral("SELECT ");
            this.Write(this.Model);
            this.WriteLiteral(" AS `Item`");
        }
    }
}
