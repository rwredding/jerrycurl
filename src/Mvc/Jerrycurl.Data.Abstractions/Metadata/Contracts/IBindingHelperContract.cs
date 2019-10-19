using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingHelperContract
    {
        object Object { get; }
        Type Type { get; }
    }
}
