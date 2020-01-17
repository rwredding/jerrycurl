using Jerrycurl.Mvc.Sql;
using Jerrycurl.Mvc.Sql.SqlServer;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.SqlServer.Test.Models;
using Jerrycurl.Vendors.SqlServer.Test.Views;
using Shouldly;
using System.Collections.Generic;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class JsonTests
    {
        public void JsonValue_IsReferencedCorrectly()
        {
            Runnable table = new Runnable();

            table.Sql(@"
DROP TABLE IF EXISTS jerry_json;
CREATE TABLE jerry_json(
        [Json] nvarchar(MAX) NOT NULL
);");

            Runner.Command(table);

            List<TestModel> testModels = new List<TestModel>()
            {
                new TestModel() { Json = new JsonModel() { Value1 = 10, Value3 = 20 } },
                new TestModel() { Json = new JsonModel() { Value1 = 20, Value3 = 30 } },
            };

            Runnable<TestModel, object> insert1 = new Runnable<TestModel, object>(testModels[0]);
            Runnable<TestModel, object> insert2 = new Runnable<TestModel, object>(testModels[1]);

            insert1.Sql("INSERT INTO jerry_json ( [Json] ) VALUES ( ");
            insert1.M(p => p.Par(m => m.Json));
            insert1.Sql(" );");

            insert2.Sql("INSERT INTO jerry_json ( [Json] ) VALUES ( ");
            insert2.M(p => p.Par(m => m.Json));
            insert2.Sql(" );");

            Runner.Command(insert1);
            Runner.Command(insert2);

            Runnable<object, JsonView> select = new Runnable<object, JsonView>();

            select.Sql("SELECT [Json] AS ");
            select.R(p => p.Prop(m => m.Json));
            select.Sql(", [Json] AS ");
            select.R(p => p.Prop(m => m.JsonString));
            select.Sql(" FROM jerry_json ");
            select.R(p => p.Ali());
            select.Sql(" WHERE ");
            select.R(p => p.Json(m => m.Json.Value1));
            select.Sql(" = 10 AND ");
            select.R(p => p.Json(m => m.Json.Value3));
            select.Sql(" = 20 ");

            IList<JsonView> result = Runner.Query(select);

            result.Count.ShouldBe(1);
            result[0].Json.Value1.ShouldBe(10);
            result[0].Json.Value3.ShouldBe(20);
        }
    }
}
