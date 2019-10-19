using Jerrycurl.Data.Queries;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.SqlServer.Test.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class TypeTests
    {
        public void TypesAndParameters_AreBoundProperly()
        {
            Runnable<object, object> table = new Runnable<object, object>();

            table.Sql(@"
DROP TABLE IF EXISTS jerry_types;
CREATE TABLE jerry_types(
        [BigInt] bigint NOT NULL,
        [Bit] bit NOT NULL,
        [SmallInt] smallint NOT NULL,
        [Real] real NOT NULL,
        [Int] int NOT NULL,
        [Float] float NOT NULL,
        [DateTimeOffset] datetimeoffset NOT NULL,
        [Date] date NOT NULL,
        [DateTime] datetime NOT NULL,
        [DateTime2] datetime2 NOT NULL,
        [SmallDateTime] smalldatetime NOT NULL,
        [Time] time NOT NULL,
        [NChar] nchar(20) NOT NULL,
        [NVarChar] nvarchar(20) NOT NULL,
        [NText] ntext NOT NULL,
        [Char] char(20) NOT NULL,
        [VarChar] varchar(20) NOT NULL,
        [Text] text NOT NULL,
        [Xml] xml NOT NULL,
        [Image] image NOT NULL,
        [Binary] binary(20) NOT NULL,
        [VarBinary] varbinary(20) NOT NULL,
        [UniqueIdentifier] uniqueidentifier NOT NULL
);");

            Runner.Command(table);

            
            Runnable<TypeModel, object> insert = new Runnable<TypeModel, object>(TypeModel.GetSample());

            insert.Sql("INSERT INTO jerry_types ( ");
            insert.M(p => p.ColNames());
            insert.Sql(" ) VALUES ( ");
            insert.M(p => p.Pars());
            insert.Sql(")");

            Runner.Command(insert);

            Runnable<object, TypeModel> select = new Runnable<object, TypeModel>();

            select.Sql("SELECT ");
            select.R(p => p.Star());
            select.Sql(" FROM jerry_types ");
            select.R(p => p.Ali());

            TypeModel sample = TypeModel.GetSample();
            TypeModel fromDb = Runner.Query(select).FirstOrDefault();

            this.CompareTypeModels(fromDb, sample);

            TypeModel fromDb2 = new TypeModel();
            Runnable<TypeModel, object> bind = new Runnable<TypeModel, object>(fromDb2);

            bind.Sql("SELECT ");
            bind.M(p => p.Cols().As().Props());
            bind.Sql(" FROM jerry_types ");
            bind.M(p => p.Ali());

            Runner.Command(bind);

            this.CompareTypeModels(fromDb2, sample);
        }

        private void CompareTypeModels(TypeModel fromDb, TypeModel sample)
        {
            fromDb.BigInt.ShouldBe(sample.BigInt);
            fromDb.Binary.ShouldBe(sample.Binary.Concat(new byte[fromDb.Binary.Length - sample.Binary.Length]).ToArray());
            fromDb.Bit.ShouldBe(sample.Bit);
            fromDb.Char.ShouldBe(sample.Char.PadRight(20));
            fromDb.Date.ShouldBe(sample.Date);
            fromDb.DateTime.ShouldBe(sample.DateTime);
            fromDb.DateTime2.ShouldBe(sample.DateTime2);
            fromDb.DateTimeOffset.ShouldBe(sample.DateTimeOffset);
            fromDb.Float.ShouldBe(sample.Float);
            fromDb.Image.ShouldBe(sample.Image);
            fromDb.Int.ShouldBe(sample.Int);
            fromDb.NChar.ShouldBe(sample.NChar.PadRight(20));
            fromDb.NText.ShouldBe(sample.NText);
            fromDb.NVarChar.ShouldBe(sample.NVarChar);
            fromDb.Real.ShouldBe(sample.Real);
            fromDb.SmallDateTime.ShouldBe(sample.SmallDateTime);
            fromDb.SmallInt.ShouldBe(sample.SmallInt);
            fromDb.Text.ShouldBe(sample.Text);
            fromDb.Time.ShouldBe(sample.Time);
            fromDb.UniqueIdentifier.ShouldBe(sample.UniqueIdentifier);
            fromDb.VarBinary.ShouldBe(sample.VarBinary);
            fromDb.VarChar.ShouldBe(sample.VarChar);
            fromDb.Xml.ToString().ShouldBe(sample.Xml.ToString());
        }
    }
}
