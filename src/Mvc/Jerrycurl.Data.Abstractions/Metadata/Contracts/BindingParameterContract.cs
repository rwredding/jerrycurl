using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Metadata
{
    public class BindingParameterContract : IBindingParameterContract
    {
        public BindingParameterWriter Write { get; set; }
        public BindingParameterConverter Convert { get; set; }
    }
}
