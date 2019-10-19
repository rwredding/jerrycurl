using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Data
{
    public interface IParameter
    {
        string Name { get; }
        IField Field { get; }
        IBindingParameterContract Contract { get; }
    }
}
