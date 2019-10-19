using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.Sql;

namespace Jerrycurl.Mvc.Test.Conventions.Queries.Crud
{
    public class Delete_cssql : ProcPage<dynamic, object>
    {
        public Delete_cssql(IProjection model, IProjection result)
            : base(model, result)
        {

        }

        public override void Execute()
        {
            foreach (var v in this.M.Vals())
            {
                this.WriteLiteral("DELETE FROM ");
                this.Write(v.TblName());
                this.WriteLiteral(" WHERE ");
                this.Write(v.Key().ColNames().IsEq().Pars());
                this.WriteLiteral(";\r\n");
            }
        }
    }
}
