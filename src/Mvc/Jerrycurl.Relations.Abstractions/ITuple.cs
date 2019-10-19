using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations
{
    public interface ITuple : IReadOnlyList<IField>, IEquatable<ITuple>
    {
        int Degree { get; }
    }
}
