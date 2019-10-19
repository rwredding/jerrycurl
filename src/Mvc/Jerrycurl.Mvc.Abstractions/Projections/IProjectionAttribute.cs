using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Commands;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Projections
{
    public interface IProjectionAttribute : ISqlWritable
    {
        IProjectionIdentity Identity { get; }
        IProcContext Context { get; }
        IProjectionMetadata Metadata { get; }
        ISqlContent Content { get; }
        Func<IField> Field { get; }

        IProjectionAttribute Append(IEnumerable<IParameter> parameters);
        IProjectionAttribute Append(IEnumerable<ICommandBinding> bindings);
        IProjectionAttribute Append(string text);
        IProjectionAttribute Append(params IParameter[] parameter);
        IProjectionAttribute Append(params ICommandBinding[] bindings);

        IProjectionAttribute With(IProjectionMetadata metadata = null,
                                  ISqlContent content = null,
                                  Func<IField> field = null);
    }
}
