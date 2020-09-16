using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class ColumnSource : IFieldSource
    {
        public ColumnMetadata Info { get; set; }
        public object Value { get; set; } = DBNull.Value;
        public bool HasChanged => (this.Info != null);
    }
}
