using System.Data;
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
