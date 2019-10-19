using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : Attribute
    {
        public IEnumerable<string> Parts { get; }

        public TableAttribute()
        {

        }

        public TableAttribute(params string[] parts)
        {
            this.Parts = parts;
        }
    }
}
