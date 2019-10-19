using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Vendors.MySql
{
    public class MySqlScaffoldCommand : ScaffoldCommand
    {
        public override DbConnection GetDbConnection() => new MySqlConnection();
        public override async Task<DatabaseModel> GetDatabaseModelAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            ModelBuilder builder = new ModelBuilder();

            using (DbCommand tablesAndColumns = connection.CreateCommand())
            {
                tablesAndColumns.CommandText = @"SELECT *
                                                 FROM INFORMATION_SCHEMA.TABLES T1
                                                 INNER JOIN INFORMATION_SCHEMA.COLUMNS T2 ON T2.TABLE_SCHEMA = T1.TABLE_SCHEMA AND T2.TABLE_NAME = T1.TABLE_NAME
                                                 WHERE T1.TABLE_SCHEMA = DATABASE() AND T1.TABLE_TYPE = 'BASE TABLE'
                                                 ORDER BY T1.TABLE_SCHEMA, T1.TABLE_NAME, T2.ORDINAL_POSITION";

                await this.AddTablesAndColumnsAsync(builder, tablesAndColumns);
            }

            using (DbCommand primaryKeys = connection.CreateCommand())
            {
                primaryKeys.CommandText = @"SELECT T2.* FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T1
                                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE T2 ON T2.CONSTRAINT_CATALOG = T1.CONSTRAINT_CATALOG AND T2.CONSTRAINT_SCHEMA = T1.CONSTRAINT_SCHEMA AND T2.CONSTRAINT_NAME = T1.CONSTRAINT_NAME
                                            WHERE T1.CONSTRAINT_SCHEMA = DATABASE() AND T1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                            ORDER BY T2.ORDINAL_POSITION";

                await this.AddPrimaryKeysAsync(builder, primaryKeys);
            }

            using (DbCommand foreignKeys = connection.CreateCommand())
            {
                foreignKeys.CommandText = @"SELECT T2.*, T1.UNIQUE_CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS T1
                                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS T2  ON T2.CONSTRAINT_CATALOG = T1.CONSTRAINT_CATALOG  AND T2.CONSTRAINT_SCHEMA = T1.CONSTRAINT_SCHEMA AND T2.CONSTRAINT_NAME = T1.CONSTRAINT_NAME
                                            WHERE T1.CONSTRAINT_SCHEMA = DATABASE()
                                            ORDER BY T2.ORDINAL_POSITION";

                await this.AddForeignKeysAsync(builder, foreignKeys);
            }

            return builder.Model;
        }

        private async Task AddTablesAndColumnsAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string typeName = tuple["DATA_TYPE"] as string;
                bool isNullable = (tuple["IS_NULLABLE"] as string == "YES");
                bool isIdentity = ((string)tuple["EXTRA"]).Contains("auto_increment");

                builder.AddColumn(null, tableName, columnName, typeName, isNullable: isNullable, isIdentity: isIdentity);
            }
        }

        private async Task AddPrimaryKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string keyName = $"PK_{tableName}";
                int keyIndex = int.Parse(tuple["ORDINAL_POSITION"]?.ToString());

                builder.AddKey(null, tableName, columnName, keyName, keyIndex);
            }
        }

        public async Task AddForeignKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string uniqueName = $"PK_{tuple["REFERENCED_TABLE_NAME"]}";
                string foreignName = tuple["CONSTRAINT_NAME"] as string;
                int keyIndex = int.Parse(tuple["ORDINAL_POSITION"]?.ToString());

                builder.AddReference(null, tableName, columnName, foreignName, uniqueName, keyIndex);
            }
        }

        public override IEnumerable<TypeMapping> GetTypeMappings()
        {
            yield return new TypeMapping("bigint", "long", true);
            yield return new TypeMapping("decimal", "decimal", true);
            yield return new TypeMapping("double", "double", true);
            yield return new TypeMapping("float", "float", true);
            yield return new TypeMapping("real", "float", true);
            yield return new TypeMapping("int", "int", true);
            yield return new TypeMapping("mediumint", "int", true);
            yield return new TypeMapping("smallint", "short", true);
            yield return new TypeMapping("year", "short", true);
            yield return new TypeMapping("bit", "bool", true);
            yield return new TypeMapping("tinyint", "byte", true);
            yield return new TypeMapping("char", "char", true);
            yield return new TypeMapping("varchar", "string", false);
            yield return new TypeMapping("tinytext", "string", false);
            yield return new TypeMapping("text", "string", false);
            yield return new TypeMapping("mediumtext", "string", false);
            yield return new TypeMapping("longtext", "string", false);
            yield return new TypeMapping("string", "string", false);
            yield return new TypeMapping("enum", "string", false);
            yield return new TypeMapping("set", "string", false);
            yield return new TypeMapping("json", "string", false);
            yield return new TypeMapping("datetime", "DateTime", true);
            yield return new TypeMapping("date", "DateTime", true);
            yield return new TypeMapping("time", "TimeSpan", true);
            yield return new TypeMapping("timestamp", "DateTimeOffset", true);
            yield return new TypeMapping("tinyblob", "byte[]", false);
            yield return new TypeMapping("blob", "byte[]", false);
            yield return new TypeMapping("mediumblob", "byte[]", false);
            yield return new TypeMapping("longblob", "byte[]", false);
            yield return new TypeMapping("binary", "byte[]", false);
            yield return new TypeMapping("varbinary", "byte[]", false);
            yield return new TypeMapping("geometry", "byte[]", false);
        }
    }
}
