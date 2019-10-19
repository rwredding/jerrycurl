using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
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
    }
}
