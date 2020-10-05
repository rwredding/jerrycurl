using System;
using System.Collections.Generic;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Mvc.V11.Projections;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.V11.Projections
{
    /// <summary>
    /// Represents an immutable projection buffer comprised of the concatenation of a collection of attributes.
    /// </summary>
    public interface IProjection2 : ISqlWritable
    {
        ProjectionHeader Header { get; }
        IProjectionIdentity Identity { get; }
        IProcContext Context { get; }
        IProjectionOptions Options { get; }
        IField Source { get; }

        IProjection Append(IEnumerable<IParameter> parameters);
        IProjection Append(IEnumerable<IUpdateBinding> bindings);
        IProjection Append(string text);
        IProjection Append(params IParameter[] parameter);
        IProjection Append(params IUpdateBinding[] bindings);

        IProjection Map(Func<IProjectionAttribute, IProjectionAttribute> map);

        IProjection With(IProjectionMetadata metadata = null,
                         IEnumerable<IProjectionAttribute> attributes = null,
                         IField field = null,
                         IProjectionOptions options = null);
    }
}
