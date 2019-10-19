using Jerrycurl.Data.Queries;
using Jerrycurl.Mvc;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.Postgres.Test.Models;
using Jerrycurl.Vendors.Postgres.Test.Views;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Vendors.Postgres.Test
{
    public class JsonTests
    {
        public void JsonParameters_GetCorrectNpgsqlType()
        {
            Runnable<object, object> table = new Runnable<object, object>();

            table.Sql(@"
DROP TABLE IF EXISTS jerry_json;
CREATE TABLE jerry_json(
        ""Json"" json NOT NULL,
        ""JsonB"" jsonb NOT NULL
);");

            Runner.Command(table);

            TestModel sample = new TestModel()
            {
                Json = new JsonModel() { Value1 = 1, Value3 = 2 },
                JsonB = new JsonModel() { Value1 = 3, Value3 = 4 },
            };

            Runnable<TestModel, object> insert = new Runnable<TestModel, object>(sample);

            insert.Sql("INSERT INTO jerry_json ( ");
            insert.M(p => p.ColNames());
            insert.Sql(" ) VALUES ( ");
            insert.M(p => p.Pars());
            insert.Sql(");");

            Runner.Command(insert);

            Runnable<object, JsonView> select = new Runnable<object, JsonView>();

            select.Sql("SELECT ");
            select.R(p => p.Star());
            select.Sql(" FROM jerry_json ");
            select.R(p => p.Ali());
            select.Sql(";");

            JsonView fromDb = Runner.Query(select).FirstOrDefault();

            fromDb.Json.ShouldNotBeNull();
            fromDb.JsonB.ShouldNotBeNull();

            fromDb.Json.Value1.ShouldBe(sample.Json.Value1);
            fromDb.Json.Value3.ShouldBe(sample.Json.Value3);
            fromDb.JsonB.Value1.ShouldBe(sample.JsonB.Value1);
            fromDb.JsonB.Value3.ShouldBe(sample.JsonB.Value3);
        }
    }
}
