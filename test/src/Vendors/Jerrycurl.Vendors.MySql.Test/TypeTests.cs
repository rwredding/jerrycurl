using Jerrycurl.Data.Queries;
using Jerrycurl.Mvc.Sql;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Test;
using Jerrycurl.Test.Project.Accessors;
using Jerrycurl.Test.Project.Models;
using Jerrycurl.Vendors.MySql.Test.Models;
using MySql.Data.MySqlClient;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Vendors.MySql.Test
{
    public class TypeTests
    {

        public void TypesAndParameters_AreBoundProperly()
        {
            Runnable<object, object> table = new Runnable<object, object>();

            table.Sql(@"
DROP TABLE IF EXISTS jerry_types;
CREATE TABLE jerry_types(
    `BigInt` bigint NOT NULL,
    `TinyInt` tinyint NOT NULL,
    `Double` double NOT NULL,
    `Int` int SIGNED NOT NULL,
    `Float` float NOT NULL,
    `Date` date NOT NULL,
    `Decimal` decimal NOT NULL,
    `DateTime` datetime NOT NULL,
    `TimeStamp` timestamp NOT NULL,
    `Time` time NOT NULL,
    `Char` char(20) NOT NULL,
    `VarChar` varchar(20) NOT NULL,
    `Text` text NOT NULL,
    `Blob` blob NOT NULL,
    `LongBlob` longblob NOT NULL,
    `MediumBlob` mediumblob NOT NULL,
    `TinyBlob` tinyblob NOT NULL,
    `MediumInt` mediumint NOT NULL,
    `LongText` longtext NOT NULL,
    `MediumText` mediumtext NOT NULL,
    `TinyText` tinytext NOT NULL,
    `SmallInt` smallint NOT NULL,
    `UBigInt` bigint UNSIGNED NOT NULL,
    `UInt` int UNSIGNED NOT NULL,
    `UMediumInt` mediumint UNSIGNED NOT NULL,
    `USmallInt` smallint UNSIGNED NOT NULL,
    `UTinyInt` tinyint UNSIGNED NOT NULL,
    `Year` year NOT NULL,
    `Binary` binary(20) NOT NULL,
    `VarBinary` varbinary(20) NOT NULL,
    `Enum` enum('Jerrycurl', 'EF') NOT NULL,
    `Set` set('Jerrycurl', 'EF') NOT NULL
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
            fromDb.Blob.ShouldBe(sample.Blob);
            fromDb.Char.ShouldBe(sample.Char);
            fromDb.Date.ShouldBe(sample.Date);
            fromDb.DateTime.ShouldBe(sample.DateTime);
            fromDb.Double.ShouldBe(sample.Double);
            fromDb.Enum.ShouldBe(sample.Enum);
            fromDb.Float.ShouldBe(sample.Float);
            fromDb.Int.ShouldBe(sample.Int);
            fromDb.LongBlob.ShouldBe(sample.LongBlob);
            fromDb.LongText.ShouldBe(sample.LongText);
            fromDb.MediumBlob.ShouldBe(sample.MediumBlob);
            fromDb.MediumInt.ShouldBe(sample.MediumInt);
            fromDb.MediumText.ShouldBe(sample.MediumText);
            fromDb.SmallInt.ShouldBe(sample.SmallInt);
            fromDb.Text.ShouldBe(sample.Text);
            fromDb.Time.ShouldBe(sample.Time);
            fromDb.TimeStamp.ShouldBe(sample.TimeStamp);
            fromDb.TinyBlob.ShouldBe(sample.TinyBlob);
            fromDb.TinyInt.ShouldBe(sample.TinyInt);
            fromDb.TinyText.ShouldBe(sample.TinyText);
            fromDb.UBigInt.ShouldBe(sample.UBigInt);
            fromDb.UInt.ShouldBe(sample.UInt);
            fromDb.UMediumInt.ShouldBe(sample.UMediumInt);
            fromDb.USmallInt.ShouldBe(sample.USmallInt);
            fromDb.UTinyInt.ShouldBe(sample.UTinyInt);
            fromDb.VarBinary.ShouldBe(sample.VarBinary);
            fromDb.VarChar.ShouldBe(sample.VarChar);
            fromDb.Year.ShouldBe(sample.Year);
            fromDb.Set.ShouldBe(sample.Set);
        }
    }
}
