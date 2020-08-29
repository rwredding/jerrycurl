using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc.Projections
{
    public interface IProjectionValues<TItem> : IEnumerable<IProjection<TItem>>
    {
        IProjectionIdentity Identity { get; }
        IProcContext Context { get; }
        IEnumerable<IProjection<TItem>> Items { get; }
    }
}
