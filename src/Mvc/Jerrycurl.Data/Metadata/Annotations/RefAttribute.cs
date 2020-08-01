using System;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class RefAttribute : Attribute
    {
        public string KeyName { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }

        public RefAttribute()
        {

        }

        public RefAttribute(string keyName)
        {
            this.KeyName = keyName;
        }

        public RefAttribute(string keyName, int index)
        {
            this.KeyName = keyName;
            this.Index = index;
        }

        public RefAttribute(string keyName, int index, string name)
        {
            this.KeyName = keyName;
            this.Index = index;
            this.Name = name;
        }
    }
}
