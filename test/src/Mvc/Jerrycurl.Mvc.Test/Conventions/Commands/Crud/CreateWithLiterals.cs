using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Crud
{
    public class CreateWithLiterals_cssql : ProcPage<dynamic, object>
    {
        public CreateWithLiterals_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            foreach (var v in this.M.Vals())
            {
                this.WriteLiteral("INSERT INTO ");
                this.Write(v.TblName());
                this.WriteLiteral(" (");
                this.Write(v.In().ColNames());
                this.WriteLiteral(" ) VALUES ( ");
                this.Write(v.In().Lits());
                this.WriteLiteral(" ); ");
                this.WriteLiteral("SELECT last_insert_rowid() AS ");
                this.Write(v.Id().Prop());
                this.WriteLiteral(";\r\n");
            }
        }
    }
}
