using System;
using System.Collections;
using System.Collections.Generic;

namespace Jerrycurl.Relations.Internal
{
    internal class IndexedEnumerator<T> : IEnumerator<T>
    {
        protected IEnumerator<T> innerEnumerator;

        public int Index { get; private set; } = -1;

        public T Current => this.innerEnumerator.Current;

        public IndexedEnumerator(IEnumerable<T> enumerable)
        {
            this.innerEnumerator = (enumerable ?? Array.Empty<T>()).GetEnumerator();
        }

        public virtual bool MoveNext()
        {
            if (this.innerEnumerator.MoveNext())
            {
                this.Index++;

                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.innerEnumerator.Reset();
            this.Index = -1;
        }

        public virtual void Dispose()
        {
            this.innerEnumerator.Dispose();
        }

        object IEnumerator.Current => this.Current;
    }
}
