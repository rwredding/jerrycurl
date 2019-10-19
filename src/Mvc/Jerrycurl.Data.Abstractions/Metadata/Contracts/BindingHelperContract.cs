using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public class BindingHelperContract<THelper> : IBindingHelperContract
        where THelper : class
    {
        public BindingHelperContract(THelper helper)
        {
            this.Object = helper;
            this.Type = typeof(THelper);
        }

        public object Object { get; }
        public Type Type { get; }
    }
}
