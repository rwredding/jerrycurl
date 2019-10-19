using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public delegate void ParameterWriter(IBindingParameterInfo parameterInfo);
    public delegate object ValueWriter(object value);
    public delegate Expression ValueReader(IBindingValueInfo valueInfo);
    public delegate MethodInfo ColumnReader(IBindingColumnInfo columnInfo);
}
