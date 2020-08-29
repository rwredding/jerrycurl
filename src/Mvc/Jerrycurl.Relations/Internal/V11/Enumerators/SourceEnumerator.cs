using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Internal.V11.Enumerators
{
    internal class SourceEnumerator<T> : IEnumerator<T>
    {
        private bool hasMoved = false;

        public T Current { get; }
        public string Name { get; }

        public SourceEnumerator(T value, IField2 source)
        {
            this.Current = value;
            this.Name = source.Identity.Name;
        }

        public virtual bool MoveNext()
        {
            if (this.hasMoved)
                return false;

            this.hasMoved = true;

            return true;
        }

        public void Reset()
        {
            this.hasMoved = false;
        }

        public virtual void Dispose()
        {

        }

        object IEnumerator.Current => this.Current;
    }
}
