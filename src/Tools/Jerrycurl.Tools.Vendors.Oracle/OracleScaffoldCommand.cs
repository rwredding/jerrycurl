using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Vendors.Oracle
{
    public class OracleScaffoldCommand : ScaffoldCommand
    {
        public override DbConnection GetDbConnection() => new OracleConnection();

        public override async Task<DatabaseModel> GetDatabaseModelAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            ModelBuilder builder = new ModelBuilder();

            using (DbCommand tablesAndColumns = connection.CreateCommand())
            {
                tablesAndColumns.CommandText = @"SELECT *
                                                 FROM user_tab_cols T1
                                                 ORDER BY T1.TABLE_NAME, T1.COLUMN_ID";

                await this.AddTablesAndColumnAsync(builder, tablesAndColumns);
            }

            using (DbCommand primaryKeys = connection.CreateCommand())
            {
                primaryKeys.CommandText = @"SELECT *
                                            FROM user_cons_columns a
                                            JOIN user_constraints c ON a.constraint_name = c.constraint_name
                                            INNER JOIN user_tables t ON t.table_name = a.table_name
                                            AND c.constraint_type IN ('P')";

                await this.AddPrimaryKeysAsync(builder, primaryKeys);
            }

            using (DbCommand foreignKeys = connection.CreateCommand())
            {
                foreignKeys.CommandText = @"SELECT x.*, c.constraint_name AS unique_constraint_name
                                            FROM all_cons_columns x,
                                                 all_cons_columns c,
                                                 all_constraints r,
                                                 user_tables t
                                                 WHERE x.constraint_name = r.constraint_name
                                                  AND t.table_name = x.table_name
                                                  AND c.constraint_name = r.r_constraint_name
                                                  AND c.owner = r.r_owner
                                                  AND r.constraint_type = 'R'
                                            ORDER BY x.position";

                await this.AddForeignKeysAsync(builder, foreignKeys);
            }

            return builder.Model;
        }

        private async Task AddTablesAndColumnAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = null;
                string tableName = (tuple["TABLE_NAME"] as string).Trim();
                string columnName = tuple["COLUMN_NAME"] as string;
                string typeName = this.GetSanitizedDataType(tuple);
                bool isNullable = (tuple["NULLABLE"] as string == "Y");
                bool isIdentity = (tuple["IDENTITY_COLUMN"] as string == "YES");
                bool ignoreTable = false;

                builder.AddColumn(tableSchema, tableName, columnName, typeName, isNullable: isNullable, isIdentity: isIdentity, ignoreTable: ignoreTable);
            }
        }

        private string GetSanitizedDataType(TupleData tuple)
        {
            string dataType = (tuple["DATA_TYPE"] as string)?.Trim();

            if (dataType != null)
                dataType = Regex.Replace(dataType, @"\(\d+\)", "");

            return dataType;
        }

        private async Task AddPrimaryKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = null;
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string keyName = tuple["CONSTRAINT_NAME"] as string;
                int keyIndex = int.Parse(tuple["POSITION"]?.ToString());

                builder.AddKey(tableSchema, tableName, columnName, keyName, keyIndex);
            }
        }

        public async Task AddForeignKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableSchema = null;
                string tableName = tuple["TABLE_NAME"] as string;
                string columnName = tuple["COLUMN_NAME"] as string;
                string uniqueName = tuple["UNIQUE_CONSTRAINT_NAME"] as string;
                string foreignName = tuple["CONSTRAINT_NAME"] as string;
                int keyIndex = int.Parse(tuple["POSITION"]?.ToString());

                builder.AddReference(tableSchema, tableName, columnName, foreignName, uniqueName, keyIndex);
            }
        }

        public override IEnumerable<TypeMapping> GetTypeMappings()
        {
            yield return new TypeMapping("BFILE", "byte[]", false);
            yield return new TypeMapping("BLOB", "byte[]", false);
            yield return new TypeMapping("CHAR", "string", false);
            yield return new TypeMapping("CLOB", "string", false);
            yield return new TypeMapping("DATE", "DateTime", true);
            yield return new TypeMapping("FLOAT", "decimal", true);
            yield return new TypeMapping("INTEGER", "decimal", true);
            yield return new TypeMapping("INTERVAL YEAR TO MONTH", "long", true);
            yield return new TypeMapping("INTERVAL DAY TO SECOND", "TimeSpan", true);
            yield return new TypeMapping("LONG", "string", false);
            yield return new TypeMapping("LONG RAW", "byte[]", false);
            yield return new TypeMapping("NCHAR", "string", false);
            yield return new TypeMapping("NCLOB", "string", false);
            yield return new TypeMapping("NUMBER", "decimal", true);
            yield return new TypeMapping("NVARCHAR2", "string", false);
            yield return new TypeMapping("RAW", "byte[]", false);
            yield return new TypeMapping("ROWID", "string", false);
            yield return new TypeMapping("TIMESTAMP", "DateTime", true);
            yield return new TypeMapping("TIMESTAMP WITH LOCAL TIME ZONE", "DateTime", true);
            yield return new TypeMapping("TIMESTAMP WITH TIME ZONE", "DateTimeOffset", true);
            yield return new TypeMapping("UNSIGNED INTEGER", "decimal", true);
            yield return new TypeMapping("VARCHAR2", "string", false);
            yield return new TypeMapping("ANYDATA", "object", false);
        }
    }
}
