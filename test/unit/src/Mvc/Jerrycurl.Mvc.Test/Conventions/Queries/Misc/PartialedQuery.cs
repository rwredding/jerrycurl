using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Misc
{
    public class PartialedQuery_cssql : ProcPage<object, object>
    {
        public PartialedQuery_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            this.WriteLiteral("SELECT 1 AS `Item` UNION ");
            this.R.Subquery("SubQuery", 2);
            this.WriteLiteral(" UNION ");
            this.R.Subquery("SubQuery.cssql", 3);
        }
    }
}
