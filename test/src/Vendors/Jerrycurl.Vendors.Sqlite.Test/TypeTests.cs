using Jerrycurl.Data.Queries;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.Sqlite.Test.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Vendors.Sqlite.Test
{
    public class TypeTests
    {
        public void TypesAndParameters_AreBoundProperly()
        {
            Runnable<object, object> table = new Runnable<object, object>();

            table.Sql(@"
DROP TABLE IF EXISTS jerry_types;
CREATE TABLE jerry_types(
        ""Integer"" integer NOT NULL,
        ""Real"" real NOT NULL,
        ""Text"" text NOT NULL,
        ""Blob"" blob NOT NULL
);");

            Runner.Command(table);
            
            Runnable<TypeModel, object> insert = new Runnable<TypeModel, object>(TypeModel.GetSample());

            insert.Sql("INSERT INTO jerry_types ( ");
            insert.M(p => p.ColNames());
            insert.Sql(" ) VALUES ( ");
            insert.M(p => p.Pars());
            insert.Sql(");");

            Runner.Command(insert);

            Runnable<object, TypeModel> select = new Runnable<object, TypeModel>();

            select.Sql("SELECT ");
            select.R(p => p.Star());
            select.Sql(" FROM jerry_types ");
            select.R(p => p.Ali());
            select.Sql(";");

            TypeModel sample = TypeModel.GetSample();
            TypeModel fromDb = Runner.Query(select).FirstOrDefault();

            this.CompareTypeModels(fromDb, sample);

            TypeModel fromDb2 = new TypeModel();
            Runnable<TypeModel, object> bind = new Runnable<TypeModel, object>(fromDb2);

            bind.Sql("SELECT ");
            bind.M(p => p.Cols().As().Props());
            bind.Sql(" FROM jerry_types ");
            bind.M(p => p.Ali());
            bind.Sql(";");

            Runner.Command(bind);

            this.CompareTypeModels(fromDb2, sample);
        }

        private void CompareTypeModels(TypeModel fromDb, TypeModel sample)
        {
            fromDb.Integer.ShouldBe(sample.Integer);
            fromDb.Real.ShouldBe(sample.Real);
            fromDb.Text.ShouldBe(sample.Text);
            fromDb.Blob.ShouldBe(sample.Blob);
        }
    }
}
