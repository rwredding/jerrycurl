using System;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingHelperContract
    {
        object Object { get; }
        Type Type { get; }
    }
}
