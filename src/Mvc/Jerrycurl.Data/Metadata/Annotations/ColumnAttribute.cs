using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; }

        public ColumnAttribute()
        {

        }

        public ColumnAttribute(string name)
        {
            this.Name = name;
        }
    }
}
