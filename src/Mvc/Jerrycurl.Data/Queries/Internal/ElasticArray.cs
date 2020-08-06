using System.Collections;
using System.Collections.Generic;

namespace Jerrycurl.Data.Queries.Internal
{
    internal class ElasticArray<TValue> : IEnumerable<TValue>
    {
        private readonly List<TValue> innerList = new List<TValue>(2);

        public TValue this[int index]
        {
            get
            {
                this.EnsureIndex(index);

                return this.innerList[index];
            }
            set
            {
                this.EnsureIndex(index);

                this.innerList[index] = value;
            }
        }

        private void EnsureIndex(int index)
        {
            if (index >= this.innerList.Count)
                this.innerList.AddRange(new TValue[index - this.innerList.Count + 1]);
        }

        public IEnumerator<TValue> GetEnumerator() => this.innerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal class ElasticArray : ElasticArray<object> { }
}