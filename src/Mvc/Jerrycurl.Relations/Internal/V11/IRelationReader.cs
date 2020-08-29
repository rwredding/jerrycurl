using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface IRelationReader : IDisposable
    {
        IRelation3 Relation { get; }
        bool Read();
        int Degree { get; }
        IField2 this[int index] { get; }

        void CopyTo(IField2[] target, int sourceIndex, int targetIndex, int length);
        void CopyTo(IField2[] target, int length);
    }
}
