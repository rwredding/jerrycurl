using Jerrycurl.Data.Queries;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Mvc.Sql.SqlServer;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.SqlServer.Test.Models;
using Jerrycurl.Vendors.SqlServer.Test.Views;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class TvpTests
    {
        public void TableValuedParameters_AreBoundCorrectly()
        {
            Runnable<object, object> table = new Runnable<object, object>();

            table.Sql(@"
IF type_id('jerry_tt') IS NOT NULL
    DROP TYPE jerry_tt;

CREATE TYPE jerry_tt AS TABLE
(
    Num1 int NOT NULL,
    Num2 int NOT NULL
);");

            Runner.Command(table);

            TestModel testModel = new TestModel()
            {
                Tvp = new List<TvpModel>()
                {
                    new TvpModel() { Num1 = 1, Num2 = 1 },
                    new TvpModel() { Num1 = 2, Num2 = 2 },
                    new TvpModel() { Num1 = 4, Num2 = 4 },
                    new TvpModel() { Num1 = 8, Num2 = 8 },
                    new TvpModel() { Num1 = 16, Num2 = 16 },
                }
            };

            Runnable<TestModel, int> select = new Runnable<TestModel, int>(testModel);

            select.Sql("SELECT ");
            select.M(p => p.Open(m => m.Tvp).Col(m => m.Num1));
            select.Sql(" * ");
            select.M(p => p.Open(m => m.Tvp).Col(m => m.Num2));
            select.Sql(" AS ");
            select.R(p => p.Prop());
            select.Sql(" FROM ");
            select.M(p => p.Tvp(m => m.Tvp));

            IList<int> result = Runner.Query(select);

            result.ShouldBe(new[] { 1, 4, 16, 64, 256 });
        }
    }
}
