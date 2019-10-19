using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class RefAttribute : Attribute
    {
        public string Key { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }

        public RefAttribute()
        {

        }

        public RefAttribute(string key)
        {
            this.Key = key;
        }

        public RefAttribute(string key, int index)
        {
            this.Key = key;
            this.Index = index;
        }

        public RefAttribute(string key, int index, string name)
        {
            this.Key = key;
            this.Index = index;
            this.Name = name;
        }
    }
}
