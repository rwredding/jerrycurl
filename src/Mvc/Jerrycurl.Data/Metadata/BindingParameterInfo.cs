using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Metadata
{
    internal class BindingParameterInfo : IBindingParameterInfo
    {
        public IBindingMetadata Metadata { get; set; }
        public IDbDataParameter Parameter { get; set; }
        public IField Field { get; set; }
    }
}
