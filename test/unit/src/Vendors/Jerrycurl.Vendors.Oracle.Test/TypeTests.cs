using Jerrycurl.Mvc.Sql;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.Oracle.Test.Models;
using Shouldly;
using System.Linq;

namespace Jerrycurl.Vendors.Oracle.Test
{
    public class TypeTests
    {
        public void Test_Binding_OfCommonTypes()
        {
            Runnable drop1 = new Runnable();
            Runnable drop2 = new Runnable();
            Runnable create1 = new Runnable();
            Runnable create2 = new Runnable();

            drop1.Sql(@"
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE ""jerry_types""';
EXCEPTION
   WHEN OTHERS THEN
      NULL;
END;");

            drop2.Sql(@"
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE ""jerry_types2""';
EXCEPTION
   WHEN OTHERS THEN
      NULL;
END;");

            create1.Sql(@"
CREATE TABLE ""jerry_types""(
        ""Char"" char(20) NOT NULL,
        ""VarChar"" varchar(20) NOT NULL,
        ""VarChar2"" varchar2(20) NOT NULL,
        ""Clob"" clob NOT NULL,
        ""NClob"" nclob NOT NULL,
        ""NChar"" nchar(20) NOT NULL,
        ""NVarChar2"" nvarchar2(20) NOT NULL,
        ""Long"" long NOT NULL,
        ""Date"" date NOT NULL,
        ""Number"" number NOT NULL,
        ""Blob"" blob NOT NULL,
        ""Raw"" raw(20) NOT NULL,
        ""TimeStamp"" timestamp NOT NULL,
        ""TimeStampTz"" timestamp with time zone NOT NULL,
        ""TimeStampLz"" timestamp with local time zone NOT NULL,
        ""IntervalDS"" interval day to second NOT NULL
)");
            create2.Sql(@"
CREATE TABLE ""jerry_types2""(
        ""LongRaw"" long raw NOT NULL
)");

            Runner.Command(drop1);
            Runner.Command(drop2);
            Runner.Command(create1);
            Runner.Command(create2);


            Runnable<TypeModel, object> insert1 = new Runnable<TypeModel, object>(TypeModel.GetSample());
            Runnable<TypeModel2, object> insert2 = new Runnable<TypeModel2, object>(TypeModel2.GetSample());

            insert1.Sql(@"INSERT INTO ""jerry_types"" ( ");
            insert1.M(p => p.ColNames());
            insert1.Sql(" ) VALUES ( ");
            insert1.M(p => p.Pars());
            insert1.Sql(")");

            insert2.Sql(@"INSERT INTO ""jerry_types2"" ( ");
            insert2.M(p => p.ColNames());
            insert2.Sql(" ) VALUES ( ");
            insert2.M(p => p.Pars());
            insert2.Sql(")");

            Runner.Command(insert1);
            Runner.Command(insert2);

            Runnable<object, TypeModel> select1 = new Runnable<object, TypeModel>();
            Runnable<object, TypeModel2> select2 = new Runnable<object, TypeModel2>();

            select1.Sql("SELECT ");
            select1.R(p => p.Star());
            select1.Sql(@" FROM ""jerry_types"" ");
            select1.R(p => p.Ali());

            select2.Sql("SELECT ");
            select2.R(p => p.Star());
            select2.Sql(@" FROM ""jerry_types2"" ");
            select2.R(p => p.Ali());

            TypeModel sample1 = TypeModel.GetSample();
            TypeModel2 sample2 = TypeModel2.GetSample();

            TypeModel fromDb1 = Runner.Query(select1).FirstOrDefault();
            TypeModel2 fromDb2 = Runner.Query(select2).FirstOrDefault();

            this.CompareTypeModels(fromDb1, sample1);
            this.CompareTypeModels(fromDb2, sample2);

            TypeModel fromDb3 = new TypeModel();
            TypeModel2 fromDb4 = new TypeModel2();

            Runnable<TypeModel, object> bind1 = new Runnable<TypeModel, object>(fromDb3);
            Runnable<TypeModel2, object> bind2 = new Runnable<TypeModel2, object>(fromDb4);

            bind1.Sql("SELECT ");
            bind1.M(p => p.Cols().As().Props());
            bind1.Sql(@" FROM ""jerry_types"" ");
            bind1.M(p => p.Ali());

            bind2.Sql("SELECT ");
            bind2.M(p => p.Cols().As().Props());
            bind2.Sql(@" FROM ""jerry_types2"" ");
            bind2.M(p => p.Ali());

            Runner.Command(bind1);
            Runner.Command(bind2);

            this.CompareTypeModels(fromDb3, sample1);
            this.CompareTypeModels(fromDb4, sample2);
        }

        private void CompareTypeModels(TypeModel fromDb, TypeModel sample)
        {
            fromDb.Char.ShouldBe(sample.Char.PadRight(20));
            fromDb.VarChar.ShouldBe(sample.VarChar);
            fromDb.VarChar2.ShouldBe(sample.VarChar2);
            fromDb.Clob.ShouldBe(sample.Clob);
            fromDb.NClob.ShouldBe(sample.NClob);
            fromDb.Number.ShouldBe(sample.Number);
            fromDb.NChar.ShouldBe(sample.NChar.PadRight(20));
            fromDb.NVarChar2.ShouldBe(sample.NVarChar2);
            fromDb.Long.ShouldBe(sample.Long);
            fromDb.Date.ShouldBe(sample.Date);
            fromDb.Blob.ShouldBe(sample.Blob);
            fromDb.Raw.ShouldBe(sample.Raw);
            fromDb.TimeStamp.ShouldBe(sample.TimeStamp);
            fromDb.TimeStampLz.ShouldBe(sample.TimeStampLz);
            fromDb.TimeStampTz.ShouldBe(sample.TimeStampTz);
            fromDb.IntervalDS.ShouldBe(sample.IntervalDS);
            //fromDb.IntervalYM.ShouldBe(sample.IntervalYM);
        }

        private void CompareTypeModels(TypeModel2 fromDb, TypeModel2 sample)
        {
            fromDb.LongRaw.ShouldBe(sample.LongRaw);
        }

    }
}
