using System;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Commands
{
    public class ColumnBinding : ICommandBinding
    {
        public string ColumnName { get; }
        public IField Field { get; }

        public ColumnBinding(string columnName, IField field)
        {
            this.ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            this.Field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public ColumnBinding(IField field)
        {
            this.Field = field ?? throw new ArgumentNullException(nameof(field));
            this.ColumnName = this.Field.Identity.Name;
        }

        public override string ToString() => this.ColumnName;
    }
}
