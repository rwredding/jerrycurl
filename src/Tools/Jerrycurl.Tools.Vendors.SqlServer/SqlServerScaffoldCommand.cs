using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Vendors.SqlServer
{
    public class SqlServerScaffoldCommand : ScaffoldCommand
    {
        public override DbConnection GetDbConnection() => new SqlConnection();

        public override async Task<DatabaseModel> GetDatabaseModelAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            ModelBuilder builder = new ModelBuilder();

            builder.Model.DefaultSchema = "dbo";

            using (DbCommand tablesAndColumns = connection.CreateCommand())
            {
                tablesAndColumns.CommandText = @"SELECT *,
                                                        COLUMNPROPERTY(OBJECT_ID(T2.TABLE_SCHEMA + '.' + T2.TABLE_NAME), T2.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
                                                 FROM INFORMATION_SCHEMA.TABLES T1
                                                 INNER JOIN INFORMATION_SCHEMA.COLUMNS T2 ON T2.TABLE_SCHEMA = T1.TABLE_SCHEMA AND T2.TABLE_NAME = T1.TABLE_NAME
                                                 WHERE T1.TABLE_TYPE = 'BASE TABLE'
                                                 ORDER BY T1.TABLE_SCHEMA, T1.TABLE_NAME, T2.ORDINAL_POSITION";

                await this.AddTablesAndColumnAsync(builder, tablesAndColumns);
            }

            using (DbCommand primaryKeys = connection.CreateCommand())
            {
                primaryKeys.CommandText = @"SELECT T2.* FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T1
                                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE T2 ON T2.CONSTRAINT_CATALOG = T1.CONSTRAINT_CATALOG AND T2.CONSTRAINT_SCHEMA = T1.CONSTRAINT_SCHEMA AND T2.CONSTRAINT_NAME = T1.CONSTRAINT_NAME
                                            WHERE T1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                            ORDER BY T2.ORDINAL_POSITION";

                await this.AddPrimaryKeysAsync(builder, primaryKeys);
            }

            using (DbCommand foreignKeys = connection.CreateCommand())
            {
                foreignKeys.CommandText = @"SELECT T2.*, T1.UNIQUE_CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS T1
                                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS T2  ON T2.CONSTRAINT_CATALOG = T1.CONSTRAINT_CATALOG  AND T2.CONSTRAINT_SCHEMA = T1.CONSTRAINT_SCHEMA AND T2.CONSTRAINT_NAME = T1.CONSTRAINT_NAME
                                            ORDER BY T2.ORDINAL_POSITION";

                await this.AddForeignKeysAsync(builder, foreignKeys);
            }

            return builder.Model;
        }

        private async Task AddTablesAndColumnAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = tuple["TABLE_SCHEMA"] as string;
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string typeName = tuple["DATA_TYPE"] as string;
                bool isNullable = (tuple["IS_NULLABLE"] as string == "YES");
                bool isIdentity = ((int)tuple["IS_IDENTITY"] == 1);
                bool ignoreTable = this.IsIgnoredTable(tableSchema, tableName);

                builder.AddColumn(tableSchema, tableName, columnName, typeName, isNullable: isNullable, isIdentity: isIdentity, ignoreTable: ignoreTable);
            }
        }

        private async Task AddPrimaryKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = tuple["TABLE_SCHEMA"] as string;
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string keyName = tuple["CONSTRAINT_NAME"] as string;
                int keyIndex = int.Parse(tuple["ORDINAL_POSITION"]?.ToString());

                builder.AddKey(tableSchema, tableName, columnName, keyName, keyIndex);
            }
        }

        public async Task AddForeignKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = tuple["TABLE_SCHEMA"] as string;
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string uniqueName = tuple["UNIQUE_CONSTRAINT_NAME"] as string;
                string foreignName = tuple["CONSTRAINT_NAME"] as string;
                int keyIndex = int.Parse(tuple["ORDINAL_POSITION"]?.ToString());

                builder.AddReference(tableSchema, tableName, columnName, foreignName, uniqueName, keyIndex);
            }
        }

        private bool IsIgnoredTable(string tableSchema, string tableName)
        {
            if (tableSchema == "dbo" && tableName == "sysdiagrams")
                return true;

            return false;
        }

        public override IEnumerable<TypeMapping> GetTypeMappings()
        {
            yield return new TypeMapping("int", "int", true);
            yield return new TypeMapping("bigint", "long", true);
            yield return new TypeMapping("smallint", "short", true);
            yield return new TypeMapping("tinyint", "byte", true);
            yield return new TypeMapping("bit", "bool", true);
            yield return new TypeMapping("date", "DateTime", true);
            yield return new TypeMapping("datetime", "DateTime", true);
            yield return new TypeMapping("datetime2", "DateTime", true);
            yield return new TypeMapping("smalldatetime", "DateTime", true);
            yield return new TypeMapping("time", "TimeSpan", true);
            yield return new TypeMapping("datetimeoffset", "DateTimeOffset", true);
            yield return new TypeMapping("nvarchar", "string", false);
            yield return new TypeMapping("varchar", "string", false);
            yield return new TypeMapping("text", "string", false);
            yield return new TypeMapping("ntext", "string", false);
            yield return new TypeMapping("char", "string", false);
            yield return new TypeMapping("nchar", "string", false);
            yield return new TypeMapping("varbinary", "byte[]", false);
            yield return new TypeMapping("binary", "byte[]", false);
            yield return new TypeMapping("image", "byte[]", false);
            yield return new TypeMapping("smallmoney", "decimal", true);
            yield return new TypeMapping("money", "decimal", true);
            yield return new TypeMapping("decimal", "decimal", true);
            yield return new TypeMapping("numeric", "decimal", true);
            yield return new TypeMapping("real", "float", true);
            yield return new TypeMapping("float", "double", true);
            yield return new TypeMapping("uniqueidentifier", "Guid", true);
            yield return new TypeMapping("geography", "Microsoft.SqlServer.Types.SqlGeography", false);
            yield return new TypeMapping("geometry", "Microsoft.SqlServer.Types.SqlGeometry", false);
            yield return new TypeMapping("hierarchyid", "Microsoft.SqlServer.Types.SqlHierarchyId", true);
            yield return new TypeMapping("sql_variant", "object", false);
            yield return new TypeMapping("xml", "System.Xml.Linq.XDocument", false);
        }
    }
}
