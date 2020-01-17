using System;
using System.Collections.Generic;

namespace Jerrycurl.Relations
{
    public interface ITuple : IReadOnlyList<IField>, IEquatable<ITuple>
    {
        int Degree { get; }
    }
}
