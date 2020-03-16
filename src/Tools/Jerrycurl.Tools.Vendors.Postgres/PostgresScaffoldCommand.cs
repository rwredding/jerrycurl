using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;
using Npgsql;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Vendors.Postgres
{
    public class PostgresScaffoldCommand : ScaffoldCommand
    {
        public override DbConnection GetDbConnection() => new NpgsqlConnection();

        public override async Task<DatabaseModel> GetDatabaseModelAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            ModelBuilder builder = new ModelBuilder();

            builder.Model.DefaultSchema = "public";

            using (DbCommand tablesAndColumns = connection.CreateCommand())
            {
                tablesAndColumns.CommandText = @"SELECT *
                                                 FROM INFORMATION_SCHEMA.TABLES T1
                                                 INNER JOIN INFORMATION_SCHEMA.COLUMNS T2 ON T2.TABLE_SCHEMA = T1.TABLE_SCHEMA AND T2.TABLE_NAME = T1.TABLE_NAME
                                                 WHERE T1.TABLE_TYPE = 'BASE TABLE' AND T1.TABLE_SCHEMA NOT IN ('information_schema','pg_catalog')
                                                 ORDER BY T1.TABLE_SCHEMA, T1.TABLE_NAME, T2.ORDINAL_POSITION";

                await this.AddTablesAndColumnAsync(builder, tablesAndColumns);
            }

            using (DbCommand primaryKeys = connection.CreateCommand())
            {
                primaryKeys.CommandText = @"SELECT T2.* FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T1
                                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE T2 ON T2.CONSTRAINT_CATALOG = T1.CONSTRAINT_CATALOG AND T2.CONSTRAINT_SCHEMA = T1.CONSTRAINT_SCHEMA AND T2.CONSTRAINT_NAME = T1.CONSTRAINT_NAME
                                            WHERE T1.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T1.TABLE_SCHEMA NOT IN ('information_schema','pg_catalog')
                                            ORDER BY T2.ORDINAL_POSITION";

                await this.AddPrimaryKeysAsync(builder, primaryKeys);
            }

            using (DbCommand foreignKeys = connection.CreateCommand())
            {
                foreignKeys.CommandText = @"SELECT T2.*, T1.UNIQUE_CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS T1
                                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS T2  ON T2.CONSTRAINT_CATALOG = T1.CONSTRAINT_CATALOG  AND T2.CONSTRAINT_SCHEMA = T1.CONSTRAINT_SCHEMA AND T2.CONSTRAINT_NAME = T1.CONSTRAINT_NAME
                                            WHERE T2.TABLE_SCHEMA NOT IN ('information_schema','pg_catalog')
                                            ORDER BY T2.ORDINAL_POSITION";

                await this.AddForeignKeysAsync(builder, foreignKeys);
            }

            return builder.Model;
        }

        private async Task AddTablesAndColumnAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = tuple["table_schema"] as string;
                string tableName = tuple["table_name"] as string;
                string columnName = tuple["column_name"] as string;
                string typeName = tuple["data_type"] as string;
                bool isNullable = (tuple["is_nullable"] as string == "YES");
                bool isIdentity = (tuple["is_identity"] as string == "YES" || tuple["serial_seq"] != null);
                bool ignoreTable = false;

                builder.AddColumn(tableSchema, tableName, columnName, typeName, isNullable: isNullable, isIdentity: isIdentity, ignoreTable: ignoreTable);
            }
        }

        private async Task AddPrimaryKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = tuple["table_schema"] as string;
                string tableName = tuple["table_name"] as string;
                string columnName = tuple["column_name"] as string;
                string keyName = tuple["constraint_name"] as string;
                int keyIndex = int.Parse(tuple["ordinal_position"]?.ToString());

                builder.AddKey(tableSchema, tableName, columnName, keyName, keyIndex);
            }
        }

        public async Task AddForeignKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = tuple["table_schema"] as string;
                string tableName = tuple["table_name"] as string;
                string columnName = tuple["column_name"] as string;
                string uniqueName = tuple["unique_constraint_name"] as string;
                string foreignName = tuple["constraint_name"] as string;
                int keyIndex = int.Parse(tuple["ordinal_position"]?.ToString());

                builder.AddReference(tableSchema, tableName, columnName, foreignName, uniqueName, keyIndex);
            }
        }

        public override IEnumerable<TypeMapping> GetTypeMappings()
        {
            yield return new TypeMapping("boolean", "bool", true);
            yield return new TypeMapping("smallint", "short", true);
            yield return new TypeMapping("integer", "int", true);
            yield return new TypeMapping("bigint", "long", true);
            yield return new TypeMapping("real", "float", true);
            yield return new TypeMapping("double precision", "double", true);
            yield return new TypeMapping("numeric", "decimal", true);
            yield return new TypeMapping("money", "decimal", true);
            yield return new TypeMapping("text", "string", false);
            yield return new TypeMapping("character varying", "string", false);
            yield return new TypeMapping("character", "string", false);
            yield return new TypeMapping("citext", "string", false);
            yield return new TypeMapping("json", "string", false);
            yield return new TypeMapping("jsonb", "string", false);
            yield return new TypeMapping("xml", "string", false);
            yield return new TypeMapping("name", "string", false);
            yield return new TypeMapping("bit", "System.Collections.BitArray", false);
            yield return new TypeMapping("hstore", "System.Collections.IDictionary<string, string>", false);
            yield return new TypeMapping("uuid", "Guid", true);
            yield return new TypeMapping("cidr", "(System.Net.IPAddress, int)", true);
            yield return new TypeMapping("inet", "System.Net.IPAddress", false);
            yield return new TypeMapping("macaddr", "System.Net.NetworkInformation.PhysicalAddress", true);
            yield return new TypeMapping("date", "DateTime", true);
            yield return new TypeMapping("timestamp", "DateTime", true);
            yield return new TypeMapping("timestamp with time zone", "DateTimeOffset", true);
            yield return new TypeMapping("timestamp without time zone", "DateTime", true);
            yield return new TypeMapping("time", "TimeSpan", true);
            yield return new TypeMapping("time with time zone", "DateTimeOffset", true);
            yield return new TypeMapping("time without time zone", "TimeSpan", true);
            yield return new TypeMapping("interval", "TimeSpan", true);
            yield return new TypeMapping("bytea", "byte[]", false);
            yield return new TypeMapping("oid", "uint", true);
            yield return new TypeMapping("xid", "uint", true);
            yield return new TypeMapping("cid", "uint", true);
            yield return new TypeMapping("oidvector", "uint[]", false);
            yield return new TypeMapping("ARRAY", "Array", false);
        }
    }
}
