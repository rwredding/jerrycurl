using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class OutAttribute : Attribute
    {

    }
}
