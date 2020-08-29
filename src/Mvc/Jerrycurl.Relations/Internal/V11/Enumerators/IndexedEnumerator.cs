using System;
using System.Collections;
using System.Collections.Generic;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.Enumerators
{
    internal class IndexedEnumerator<T> : IEnumerator<T>
    {
        protected IEnumerator<T> innerEnumerator;

        public string ItemName { get; }
        public IMetadataNotation Notation { get; }
        public int Index { get; private set; } = -1;
        public string Name { get; private set; }
        public T Current => this.innerEnumerator.Current;

        public IndexedEnumerator(IEnumerable<T> enumerable, string itemName, IMetadataNotation notation)
        {
            this.innerEnumerator = (enumerable ?? Array.Empty<T>()).GetEnumerator();
            this.ItemName = itemName;
            this.Notation = notation;
        }

        public virtual bool MoveNext()
        {
            if (this.innerEnumerator.MoveNext())
            {
                this.Index++;
                this.Name = this.Notation.Index(this.ItemName, this.Index);

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
