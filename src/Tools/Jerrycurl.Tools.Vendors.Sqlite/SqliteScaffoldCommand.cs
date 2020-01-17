using Jerrycurl.Tools.Scaffolding;
using Jerrycurl.Tools.Scaffolding.Model;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Jerrycurl.Tools.Vendors.Sqlite
{
    public class SqliteScaffoldCommand : ScaffoldCommand
    {
        public override DbConnection GetDbConnection() => new SqliteConnection();
        public override async Task<DatabaseModel> GetDatabaseModelAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            ModelBuilder builder = new ModelBuilder();

            using (DbCommand tablesAndColumns = connection.CreateCommand())
            {
                string systemTables = string.Join(",", this.GetSystemTableNames().Select(t => $"'{t}'"));

                if (this.HasSequenceTable(connection))
                {
                    tablesAndColumns.CommandText = @"SELECT m.name AS tbl_name, ti.*, m.sql,
                                                    (SELECT COUNT(*) FROM sqlite_sequence s WHERE s.name = m.name) AS autoincr
                                                    FROM sqlite_master AS m
                                                    JOIN pragma_table_info(m.name) AS ti
                                                    WHERE m.type = 'table'
                                                    ORDER BY m.name, ti.cid";
                }
                else
                {
                    tablesAndColumns.CommandText = @"SELECT m.name AS tbl_name, ti.*, 0 AS autoincr, m.sql
                                                    FROM sqlite_master AS m
                                                    JOIN pragma_table_info(m.name) AS ti
                                                    WHERE m.type = 'table'
                                                    ORDER BY m.name, ti.cid";
                }


                await this.AddTablesAndColumnsAsync(builder, tablesAndColumns);
            }

            using (DbCommand foreignKeys = connection.CreateCommand())
            {
                string systemTables = string.Join(",", this.GetSystemTableNames().Select(t => $"'{t}'"));

                foreignKeys.CommandText = $@"SELECT m.tbl_name, fks.*
                                             FROM sqlite_master AS m
                                             JOIN pragma_foreign_key_list(m.name) AS fks
                                             WHERE m.type = 'table'
                                             ORDER BY fks.seq";

                await this.AddForeignKeysAsync(builder, foreignKeys);
            }



            return builder.Model;
        }

        private bool HasSequenceTable(DbConnection connection)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='sqlite_sequence'";

                return (command.ExecuteScalar() != null);
            }
        }

        private async Task AddTablesAndColumnsAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string sqlDef = tuple["sql"] as string;

                string tableName = tuple["tbl_name"] as string;
                string columnName = tuple["name"] as string;
                string typeName = this.GetNormalizedTypeName(tuple);
                bool isPrimaryKey = (long)tuple["pk"] == 1;
                bool isAutoIncrement = isPrimaryKey && ((long)tuple["autoincr"] > 0 || this.HasAutoIncrementInSqlDefinition(columnName, sqlDef));
                bool isNullable = (!isPrimaryKey && (long)tuple["notnull"] == 0);
                bool ignoreTable = this.IsIgnoredTable(tableName);

                builder.AddColumn(null, tableName, columnName, typeName, isNullable, isIdentity: isAutoIncrement, ignoreTable: ignoreTable);

                if (isPrimaryKey)
                    builder.AddKey(null, tableName, columnName, "pk_" + tableName, 1);
            }
        }

        public async Task AddForeignKeysAsync(ModelBuilder builder, DbCommand command)
        {
            foreach (TupleData tuple in await TupleData.FromDbCommandAsync(command))
            {
                string tableName = tuple["tbl_name"] as string;
                string columnName = tuple["from"] as string;
                string uniqueName = $"pk_{tuple["table"]}";
                string foreignName = $"fk_{tableName}_{tuple["table"]}_{tuple["id"]}";
                int keyIndex = (int)(long)tuple["seq"];

                builder.AddReference(null, tableName, columnName, foreignName, uniqueName, keyIndex);
            }
        }

        private bool IsIgnoredTable(string tableName) => this.GetSystemTableNames().Contains(tableName);

        private IEnumerable<string> GetSystemTableNames()
        {
            return new[]
            {
                "ElementaryGeometries",
                "geometry_columns",
                "geometry_columns_auth",
                "geometry_columns_field_infos",
                "geometry_columns_statistics",
                "geometry_columns_time",
                "spatial_ref_sys",
                "spatial_ref_sys_aux",
                "SpatialIndex",
                "spatialite_history",
                "sql_statements_log",
                "views_geometry_columns",
                "views_geometry_columns_auth",
                "views_geometry_columns_field_infos",
                "views_geometry_columns_statistics",
                "virts_geometry_columns",
                "virts_geometry_columns_auth",
                "geom_cols_ref_sys",
                "spatial_ref_sys_all",
                "virts_geometry_columns_field_infos",
                "virts_geometry_columns_statistics",
                "sqlite_sequence",
                "sqlite_stat1",
            };
        }

        private string GetUnpingedNameFromDefinition(string def)
        {
            if (def.Length == 0)
                return "";

            def = def.Trim();

            char[] pings = new[] { '"', '\'', '`' };

            foreach (char ping in pings)
            {
                int i2 = def.IndexOf(ping, 1);

                if (def[0] == ping && i2 > -1)
                    return def.Substring(1, i2 - 1);
            }

            int i3 = def.Count(c => !char.IsWhiteSpace(c));

            return def.Substring(0, i3);
        }

        private bool HasAutoIncrementInSqlDefinition(string columnName, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            int i1 = sql.IndexOf('(');
            int i2 = sql.LastIndexOf(')');

            if (i1 > -1 && i2 > i1)
            {
                string def = sql.Substring(i1 + 1, i2 - i1 - 1);

                foreach (string colDef in def.Split(','))
                {
                    string matchName = this.GetUnpingedNameFromDefinition(colDef);

                    if (matchName.Equals(columnName, StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(colDef, @"INTEGER\s+PRIMARY\s+KEY\s+AUTOINCREMENT"))
                        return true;
                }
            }

            return false;
        }

        private string GetNormalizedTypeName(TupleData tuple)
        {
            if (tuple["type"] is string typeName)
            {
                int sizeIndex = typeName.IndexOf('(');

                if (sizeIndex == -1)
                    return typeName;

                return typeName.Remove(sizeIndex);
            }

            return null;
        }

        public override IEnumerable<TypeMapping> GetTypeMappings()
        {
            // INTEGER affinity
            yield return new TypeMapping("int", "int", true);
            yield return new TypeMapping("integer", "int", true);
            yield return new TypeMapping("tinyint", "byte", true);
            yield return new TypeMapping("smallint", "short", true);
            yield return new TypeMapping("mediumint", "int", true);
            yield return new TypeMapping("bigint", "long", true);
            yield return new TypeMapping("unsigned big int", "ulong", true);
            yield return new TypeMapping("int2", "short", true);
            yield return new TypeMapping("int4", "int", true);
            yield return new TypeMapping("int8", "long", true);

            // TEXT affinity
            yield return new TypeMapping("character", "string", false);
            yield return new TypeMapping("varchar", "string", false);
            yield return new TypeMapping("varying character", "string", false);
            yield return new TypeMapping("nchar", "string", false);
            yield return new TypeMapping("native character", "string", false);
            yield return new TypeMapping("nvarchar", "string", false);
            yield return new TypeMapping("text", "string", false);
            yield return new TypeMapping("clob", "string", false);

            // BLOB affinity
            yield return new TypeMapping("blob", "byte[]", false);

            // FLOATING 
            yield return new TypeMapping("float", "float", false);
            yield return new TypeMapping("real", "float", false);
            yield return new TypeMapping("double", "double", true);
            yield return new TypeMapping("double precision", "double", true);

            // NUMERIC
            yield return new TypeMapping("numeric", "decimal", true);
            yield return new TypeMapping("decimal", "decimal", true);
            yield return new TypeMapping("boolean", "bool", true);
            yield return new TypeMapping("datetime", "DateTime", true);
            yield return new TypeMapping("date", "DateTime", true);
        }
    }
}
