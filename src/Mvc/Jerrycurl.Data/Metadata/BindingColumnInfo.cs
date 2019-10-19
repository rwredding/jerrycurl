using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Metadata
{
    internal class BindingColumnInfo : IBindingColumnInfo
    {
        public ColumnIdentity Column { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public bool CanBeNull { get; set; }
    }
}
