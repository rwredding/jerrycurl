using Jerrycurl.Mvc.Sql;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.Postgres.Test.Models;
using Shouldly;
using System.Linq;

namespace Jerrycurl.Vendors.Postgres.Test
{
    public class TypeTests
    {
        public void Test_Binding_OfCommonTypes()
        {
            Runnable table = new Runnable();

            table.Sql(@"
DROP TABLE IF EXISTS jerry_types;
CREATE TABLE jerry_types(
        ""Char"" char(20) NOT NULL,
        ""VarChar"" varchar(20) NOT NULL,
        ""Text"" text NOT NULL,
        ""SmallInt"" smallint NOT NULL,
        ""Integer"" integer NOT NULL,
        ""Real"" float NOT NULL,
        ""Double"" double precision NOT NULL,
        ""Numeric"" numeric NOT NULL,
        ""Date"" date NOT NULL,
        ""TimeStamp"" timestamp NOT NULL,
        ""TimeStampTz"" timestamptz NOT NULL,
        ""Time"" time NOT NULL,
        ""Interval"" interval NOT NULL,
        ""Uuid"" uuid NOT NULL,
        ""ArrayOfInt"" int[] NOT NULL,
        ""ArrayOfVarChar"" varchar(20)[] NOT NULL,
        ""Bytea"" bytea NOT NULL,
        ""BigInt"" bigint NOT NULL,
        ""Money"" money NOT NULL,
        ""Xml"" xml NOT NULL,
        ""Boolean"" boolean NOT NULL,
        ""Macaddr"" macaddr NOT NULL,
        ""Cidr"" cidr NOT NULL,
        ""Inet"" inet NOT NULL
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
            fromDb.Char.ShouldBe(sample.Char.PadRight(20));
            fromDb.VarChar.ShouldBe(sample.VarChar);
            fromDb.Text.ShouldBe(sample.Text);
            fromDb.SmallInt.ShouldBe(sample.SmallInt);
            fromDb.Integer.ShouldBe(sample.Integer);
            fromDb.Double.ShouldBe(sample.Double);
            fromDb.Real.ShouldBe(sample.Real);
            fromDb.Numeric.ShouldBe(sample.Numeric);
            fromDb.Date.ShouldBe(sample.Date);
            fromDb.TimeStamp.ShouldBe(sample.TimeStamp);
            fromDb.TimeStampTz.ShouldBe(sample.TimeStampTz);
            fromDb.Time.ShouldBe(sample.Time);
            fromDb.Uuid.ShouldBe(sample.Uuid);
            fromDb.ArrayOfInt.ShouldBe(sample.ArrayOfInt);
            fromDb.ArrayOfVarChar.ShouldBe(sample.ArrayOfVarChar);
            fromDb.Bytea.ShouldBe(sample.Bytea);
            fromDb.BigInt.ShouldBe(sample.BigInt);
            fromDb.Boolean.ShouldBe(sample.Boolean);
            fromDb.Money.ShouldBe(sample.Money);
            fromDb.Xml.ToString().ShouldBe(sample.Xml.ToString());
            fromDb.Interval.ShouldBe(sample.Interval);
            fromDb.Cidr.Item1.ShouldBe(sample.Cidr.Item1);
            fromDb.Cidr.Item2.ShouldBe(sample.Cidr.Item2);
            fromDb.Inet.ShouldBe(sample.Inet);
            fromDb.Macaddr.ShouldBe(sample.Macaddr);
        }
    }
}
