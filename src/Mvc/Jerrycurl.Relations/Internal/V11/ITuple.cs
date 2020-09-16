using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface ITuple2 : IReadOnlyList<IField>
    {
        int Degree { get; }
    }
}
