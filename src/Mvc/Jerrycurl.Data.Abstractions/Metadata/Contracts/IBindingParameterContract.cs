using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public delegate void BindingParameterWriter(IBindingParameterInfo parameterInfo);
    public delegate object BindingParameterConverter(object value);

    public interface IBindingParameterContract
    {
        BindingParameterWriter Write { get; }
        BindingParameterConverter Convert { get; }
    }
}
