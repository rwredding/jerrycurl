using Jerrycurl.Tools.Scaffolding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Scaffolding
{
    public class ModelBuilder
    {
        public DatabaseModel Model { get; }

        public ModelBuilder()
            : this(null)
        {

        }

        public ModelBuilder(DatabaseModel model)
        {
            this.Model = model ?? new DatabaseModel();
        }

        public TableModel AddTable(string tableSchema, string tableName, bool ignore = false)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            TableModel table = this.FindTable(tableSchema, tableName);

            if (table != null)
                return table;

            this.Model.Tables.Add(table = new TableModel()
            {
                Name = tableName,
                Schema = tableSchema,
                Ignore = ignore,
            });

            return table;
        }

        public ColumnModel AddColumn(string tableSchema, string tableName, string columnName, string typeName = null, bool isNullable = true, bool isIdentity = false, bool ignore = false, bool ignoreTable = false)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            TableModel table = this.FindTable(tableSchema, tableName);
            ColumnModel column = this.FindColumn(table, columnName);

            if (column != null)
                return column;
            else if (table == null)
                table = this.AddTable(tableSchema, tableName, ignoreTable);

            table.Columns.Add(column = new ColumnModel()
            {
                Name = columnName,
                TypeName = typeName,
                IsNullable = isNullable,
                IsIdentity = isIdentity,
                Ignore = ignore,
            });

            return column;
        }

        public KeyModel AddKey(string tableSchema, string tableName, string columnName, string keyName, int keyIndex)
        {
            TableModel table = this.FindTable(tableSchema, tableName);
            ColumnModel column = this.FindColumn(table, columnName);
            KeyModel key = this.FindKey(column, keyName, keyIndex);

            if (key != null)
                return key;
            else if (column != null)
                column = this.AddColumn(tableSchema, tableName, columnName);

            column.Keys.Add(key = new KeyModel()
            {
                Name = keyName,
                Index = keyIndex,
            });

            return key;
        }

        public ReferenceModel AddReference(string tableSchema, string tableName, string columnName, string referenceName, string keyName, int keyIndex)
        {
            TableModel table = this.FindTable(tableSchema, tableName);
            ColumnModel column = this.FindColumn(table, columnName);
            ReferenceModel reference = this.FindReference(column, referenceName, keyIndex);

            if (reference != null)
                return reference;
            else if (column != null)
                column = this.AddColumn(tableSchema, tableName, columnName);

            column.References.Add(reference = new ReferenceModel()
            {
                Name = referenceName,
                KeyName = keyName,
                KeyIndex = keyIndex,
            });

            return reference;
        }

        protected TableModel FindTable(string tableSchema, string tableName) => this.Model.Tables.FirstOrDefault(t => string.Equals(t.Schema, tableSchema) && t.Name.Equals(tableName));
        protected ColumnModel FindColumn(TableModel table, string columnName) => table?.Columns.FirstOrDefault(c => c.Name.Equals(columnName));
        protected KeyModel FindKey(ColumnModel column, string keyName, int keyIndex) => column?.Keys.FirstOrDefault(k => k.Name.Equals(keyName) && k.Index == keyIndex);
        protected ReferenceModel FindReference(ColumnModel column, string referenceName, int keyIndex) => column?.References.FirstOrDefault(r => r.Name.Equals(referenceName) && r.KeyIndex == keyIndex);
    }
}
