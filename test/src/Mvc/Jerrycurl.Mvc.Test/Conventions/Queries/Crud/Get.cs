using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Crud
{
    public class Get_cssql : ProcPage<dynamic, object>
    {
        public Get_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            this.WriteLiteral("SELECT ");
            this.Write(this.R.Star());
            this.WriteLiteral(" FROM ");
            this.Write(this.R.Tbl());
        }
    }
}
