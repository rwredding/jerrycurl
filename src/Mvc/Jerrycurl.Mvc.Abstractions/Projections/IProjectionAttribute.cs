using System;
using System.Collections.Generic;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;

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
        IProjectionAttribute Append(IEnumerable<IUpdateBinding> bindings);
        IProjectionAttribute Append(string text);
        IProjectionAttribute Append(params IParameter[] parameter);
        IProjectionAttribute Append(params IUpdateBinding[] bindings);

        IProjectionAttribute With(IProjectionMetadata metadata = null,
                                  ISqlContent content = null,
                                  Func<IField> field = null);
    }
}
