using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Crud
{
    public class Update_cssql : ProcPage<dynamic, object>
    {
        public Update_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            foreach (var v in this.M.Vals())
            {
                this.WriteLiteral("UPDATE ");
                this.Write(v.TblName());
                this.WriteLiteral(" SET ");
                this.Write(v.In().ColNames().Eq().Pars());
                this.WriteLiteral(" WHERE ");
                this.Write(v.Key().ColNames().IsEq().Pars());
                this.WriteLiteral(";\r\n");
            }
        }
    }
}
