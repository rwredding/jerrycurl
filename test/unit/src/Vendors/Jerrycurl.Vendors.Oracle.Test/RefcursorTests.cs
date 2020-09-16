using Jerrycurl.Data.Queries;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Mvc.Sql.Oracle;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Oracle.ManagedDataAccess.Client;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jerrycurl.Vendors.Oracle.Test
{
    public class RefcursorTests
    {
        public async Task Test_MultiSelect_WithRefcursors()
        {
            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () => new OracleConnection("DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCLCDB.localdomain)));USER ID=sys;PASSWORD=Oradoc_db1;DBA Privilege=SYSDBA"),
                Schemas = DatabaseHelper.Default.Schemas,
            };

            Query query = new Query()
            {
                QueryText = "BEGIN OPEN :P0 FOR SELECT 1 AS Item FROM dual; OPEN :P1 FOR SELECT 2 AS Item FROM dual; END;",
                Parameters = new IParameter[] { new Refcursor("P0"), new Refcursor("P1") },
            };

            QueryEngine handler = new QueryEngine(options);

            IList<int> result1 = handler.List<int>(query);
            IList<int> result2 = await handler.ListAsync<int>(query);

            result1.ShouldBe(new[] { 1, 2 });
            result2.ShouldBe(new[] { 1, 2 });
        }

        public void Test_MultiSelect_WithRefcursorsOnRazor()
        {
            Runnable<object, int> select = new Runnable<object, int>();

            select.Sql("BEGIN OPEN ");
            select.R(m => m.Refcursor());
            select.Sql(" FOR SELECT 1 AS ");
            select.R(m => m.Prop());
            select.Sql(" FROM dual; OPEN ");
            select.R(m => m.Refcursor());
            select.Sql(" FOR SELECT 2 AS ");
            select.R(m => m.Prop());
            select.Sql(" FROM dual; END;");

            IList<int> result1 = Runner.Query(select);

            result1.ShouldBe(new[] { 1, 2 });
        }
        public async Task Test_SingleSelect_WithoutRefcursor()
        {
            Query query = new Query()
            {
                QueryText = "SELECT 1 AS Item",
            };

            IList<int> result1 = DatabaseHelper.Default.Queries.List<int>(query);
            IList<int> result2 = await DatabaseHelper.Default.Queries.ListAsync<int>(query);

            result1.ShouldBe(new[] { 1 });
            result2.ShouldBe(new[] { 1 });
        }
    }
}
