using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface IRelationReader : IDisposable
    {
        IRelation2 Relation { get; }
        bool Read();
        IField2 this[int index] { get; }
    }
}
