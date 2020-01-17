using System;

namespace Jerrycurl.Mvc.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TemplateAttribute : Attribute
    {
        public string ProcName { get; set; }

        public TemplateAttribute()
        {

        }

        public TemplateAttribute(string procName)
        {
            this.ProcName = procName;
        }
    }
}
