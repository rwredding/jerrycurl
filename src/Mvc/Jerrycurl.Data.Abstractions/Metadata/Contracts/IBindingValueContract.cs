using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public delegate Expression BindingValueConverter(IBindingValueInfo valueInfo);
    public delegate MethodInfo BindingColumnReader(IBindingColumnInfo columnInfo);

    public interface IBindingValueContract
    {
        BindingColumnReader Read { get; }
        BindingValueConverter Convert { get; }
    }
}
