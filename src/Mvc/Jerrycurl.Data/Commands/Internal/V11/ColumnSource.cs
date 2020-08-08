using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Commands.Internal.V11
{
    internal class ColumnSource : IFieldSource
    {
        public ColumnInfo Column { get; set; }
        public object Value { get; set; } = DBNull.Value;
        public bool HasChanged => (this.Column != null);
    }
}
