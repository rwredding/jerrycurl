using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public class BindingCompositionContract : IBindingCompositionContract
    {
        public MethodInfo Add { get; set; }
        public NewExpression Construct { get; set; }
        public MethodInfo AddDynamic { get; set; }
    }
}
