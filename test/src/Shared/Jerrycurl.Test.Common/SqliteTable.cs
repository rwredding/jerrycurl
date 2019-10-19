using Jerrycurl.Data;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jerrycurl.Test
{
    public class SqliteTable : Collection<object[]>
    {
        public IEnumerable<string> Heading => this.heading;

        private readonly string[] heading;

        public SqliteTable(params string[] heading)
        {
            this.heading = heading ?? throw new ArgumentNullException(nameof(heading));
        }

        protected override void InsertItem(int index, object[] item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            else if (item.Length != this.heading.Length)
                throw new InvalidOperationException($"Invalid cardinality for SQLite table. Each row must contain {this.heading.Length} columns.");

            base.InsertItem(index, item);
        }

        public CommandData ToCommand(IEnumerable<IParameter> parameters = null, IEnumerable<ICommandBinding> bindings = null)
        {
            return new CommandData()
            {
                CommandText = this.ToSql(),
                Parameters = parameters?.ToArray() ?? Array.Empty<IParameter>(),
                Bindings = bindings?.ToArray() ?? Array.Empty<ICommandBinding>(),
            };
        }

        public QueryData ToQuery(IEnumerable<IParameter> parameters = null)
        {
            return new QueryData()
            {
                QueryText = this.ToSql(),
                Parameters = parameters?.ToArray() ?? Array.Empty<IParameter>(),
            };
        }

        private string GetSelectFromRow(object[] row)
        {
            return "SELECT " + string.Join(",\r\n", this.Heading.Zip(row, (h, v) => $"{this.ToLiteral(v)} AS `{h}`"));
        }

        private string ToLiteral(object value) => value switch
        {
            int i => i.ToString(CultureInfo.InvariantCulture),
            string s when s.StartsWith("@") => s,
            string s => "'" + s + "'",
            null => "NULL",
            _ => throw new NotSupportedException($"Type '{value.GetType().Name}' is not supported."),
        };

        public string ToSql() => string.Join("\r\nUNION ALL\r\n", this.Select(this.GetSelectFromRow)) + ";\r\n\r\n";
    }
}
