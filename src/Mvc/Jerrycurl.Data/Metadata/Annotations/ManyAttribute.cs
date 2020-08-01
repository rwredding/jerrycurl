using System;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    [Obsolete("Deprecated in 1.1. Removed in 1.2.")]
    public class ManyAttribute : Attribute
    {

    }
}
