using System;
using System.Linq.Expressions;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingValueInfo
    {
        bool CanBeNull { get; }
        bool CanBeDbNull { get; }
        IBindingMetadata Metadata { get; }
        Expression Value { get; }
        Expression Helper { get; }
        Type SourceType { get; }
        Type TargetType { get; }
    }
}
