using System;

namespace Jerrycurl.Data.Metadata.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    [Obsolete("Deprecated in 1.1. Removed in 1.2.")]
    public class AggregateAttribute : Attribute
    {

    }
}
