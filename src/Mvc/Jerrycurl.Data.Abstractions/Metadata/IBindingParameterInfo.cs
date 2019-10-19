using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Metadata
{
    public interface IBindingParameterInfo
    {
        IBindingMetadata Metadata { get; }
        IDbDataParameter Parameter { get; }
        IField Field { get; }
    }
}
