using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingCompositionContract
    {
        MethodInfo Add { get; }
        NewExpression Construct { get; }
        MethodInfo AddDynamic { get; }
    }
}
