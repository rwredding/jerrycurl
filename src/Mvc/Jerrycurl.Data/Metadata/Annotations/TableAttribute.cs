using System;
using System.Collections.Generic;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
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
