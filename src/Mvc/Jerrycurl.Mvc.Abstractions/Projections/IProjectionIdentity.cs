using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc.Projections
{
    public interface IProjectionIdentity : IEquatable<IProjectionIdentity>
    {
        IField Field { get; }
        ISchema Schema { get; }
    }
}
