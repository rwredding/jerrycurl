using System;
using System.Collections.Generic;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.Projections
{
    /// <summary>
    /// Represents an immutable projection buffer comprised of the concatenation of a collection of attributes.
    /// </summary>
    public interface IProjection : ISqlWritable
    {
        IProcContext Context { get; }
        IProjectionIdentity Identity { get; }

        IProjectionMetadata Metadata { get; }
        IProjectionOptions Options { get; }
        IField Source { get; }
        IEnumerable<IProjectionAttribute> Attributes { get; }

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
