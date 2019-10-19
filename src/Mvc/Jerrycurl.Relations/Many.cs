using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations
{
    public class Many<T> : IList<T>
    {
        private readonly List<T> innerList = new List<T>();

        public bool HasValue { get; private set; }
        public T Value => this.innerList.Count > 0 ? this.innerList[0] : default;

        T IList<T>.this[int index] { get => this.innerList[index]; set => this.innerList[index] = value; }
        int ICollection<T>.Count => this.innerList.Count;
        bool ICollection<T>.IsReadOnly => ((ICollection<T>)this.innerList).IsReadOnly;

        void ICollection<T>.Add(T item)
        {
            this.HasValue = true;
            this.innerList.Add(item);
        }

        void ICollection<T>.Clear()
        {
            this.HasValue = false;
            this.innerList.Clear();
        }

        bool ICollection<T>.Remove(T item)
        {
            bool result = this.innerList.Remove(item);

            this.HasValue = (this.innerList.Count > 0);

            return result;
        }

        void IList<T>.RemoveAt(int index)
        {
            this.innerList.RemoveAt(index);
            this.HasValue = (this.innerList.Count > 0);
        }

        bool ICollection<T>.Contains(T item) => this.innerList.Contains(item);
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => this.innerList.CopyTo(array, arrayIndex);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.innerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.innerList).GetEnumerator();

        int IList<T>.IndexOf(T item) => this.innerList.IndexOf(item);
        void IList<T>.Insert(int index, T item) => this.innerList.Insert(0, item);


    }
}
