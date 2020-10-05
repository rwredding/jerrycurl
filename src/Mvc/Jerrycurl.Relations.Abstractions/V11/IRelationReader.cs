using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.V11
{
    public interface IRelationReader : ITuple2, IDisposable
    {
        IRelation2 Relation { get; }
        bool Read();
        bool NextResult();

        void CopyTo(IField2[] target, int sourceIndex, int targetIndex, int length);
        void CopyTo(IField2[] target, int length);

    }
}
