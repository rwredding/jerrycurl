using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public class BindingValueContract : IBindingValueContract
    {
        public BindingColumnReader Read { get; set; }
        public BindingValueConverter Convert { get; set; }
    }
}
