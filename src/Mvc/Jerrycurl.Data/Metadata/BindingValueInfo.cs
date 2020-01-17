using System;
using System.Linq.Expressions;

namespace Jerrycurl.Data.Metadata
{
    internal class BindingValueInfo : IBindingValueInfo
    {
        public bool CanBeNull { get; set; }
        public bool CanBeDbNull { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public Expression Value { get; set; }
        public Expression Helper { get; set; }
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
    }
}
