using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11.Internal.Enumerators
{
    internal interface IRelationQueue : IDisposable
    {
        bool Read();
    }
}
