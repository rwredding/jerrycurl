using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
