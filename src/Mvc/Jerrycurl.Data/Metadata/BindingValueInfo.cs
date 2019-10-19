using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;

namespace Jerrycurl.Data.Metadata
{
    internal class BindingValueInfo : IBindingValueInfo
    {
        public bool CanBeNull { get; set; }
        public bool CanBeDbNull { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public Expression Value { get; set; }
        public Expression Helper { get; set; }
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
    }
}
