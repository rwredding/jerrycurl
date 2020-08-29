using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11.Enumerators
{
    internal interface IRelationQueue : IDisposable
    {
        bool Read();
    }
}
